using Amazon.Lambda.Core;
using Amazon.Lambda.DynamoDBEvents;
using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using Neo4j.Driver;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace DynamoDbNeo4jSyncer;

public class Function : IAsyncDisposable
{
    private static IDriver _driver;

    static Function()
    {
        var secretArn = Environment.GetEnvironmentVariable("NEO4J_SECRET_ARN")!;
        var initTask = InitializeDriver(secretArn);
        _driver = initTask.GetAwaiter().GetResult();
    }

    private static async Task<IDriver> InitializeDriver(string secretArn)
    {
        using var secretsClient = new AmazonSecretsManagerClient();
        var secret = await secretsClient.GetSecretValueAsync(new GetSecretValueRequest { SecretId = secretArn });
        using var jsonDoc = JsonDocument.Parse(secret.SecretString);
        var root = jsonDoc.RootElement;

        var uri = root.GetProperty("uri").GetString()!;
        var user = root.GetProperty("username").GetString()!;
        var password = root.GetProperty("password").GetString()!;

        return GraphDatabase.Driver(uri, AuthTokens.Basic(user, password));
    }

    public async Task FunctionHandler(DynamoDBEvent dynamoEvent, ILambdaContext context)
    {
        await using var session = _driver.AsyncSession();

        foreach (var record in dynamoEvent.Records)
        {
            if (record.EventName != "INSERT" && record.EventName != "MODIFY") continue;

            var newImage = record.Dynamodb.NewImage;
            if (!newImage.TryGetValue("Type", out var itemTypeAttr)) continue;

            var pk = newImage["PK"].S;
            var sk = newImage["SK"].S;

            string cypherQuery = "";
            var parameters = new Dictionary<string, object>();

            switch (itemTypeAttr.S)
            {
                case "UserProfile":
                    cypherQuery = @"MERGE (u:User { pk: $pk }) ON CREATE SET u.username = $username, u.createdAt = timestamp() ON MATCH SET u.username = $username";
                    parameters.Add("pk", pk);
                    parameters.Add("username", newImage["Username"].S);
                    break;
                case "Follow":
                    var followingUsername = sk.Replace("FOLLOWS#", "");
                    var followingPk = $"USER#{followingUsername}";
                    cypherQuery = @"MATCH (follower:User { pk: $followerPk }) MATCH (following:User { pk: $followingPk }) MERGE (follower)-[:FOLLOWS]->(following)";
                    parameters.Add("followerPk", pk);
                    parameters.Add("followingPk", followingPk);
                    break;
            }

            if (!string.IsNullOrEmpty(cypherQuery))
            {
                try
                {
                    await session.ExecuteWriteAsync(async tx => await tx.RunAsync(cypherQuery, parameters));
                    context.Logger.LogInformation($"Successfully executed Cypher for {itemTypeAttr.S}");
                }
                catch (Exception ex)
                {
                    context.Logger.LogError($"Failed to execute Cypher for {itemTypeAttr.S} ({pk}). Exception: {ex}");
                }
            }
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_driver != null)
        {
            await _driver.DisposeAsync();
        }
        GC.SuppressFinalize(this);
    }
}