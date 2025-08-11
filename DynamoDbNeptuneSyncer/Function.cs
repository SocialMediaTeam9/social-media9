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

namespace DynamoDbNeptuneSyncer;

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

            context.Logger.LogInformation($"--- Processing Record ---");
            context.Logger.LogInformation($"Event Name: {record.EventName}");
            context.Logger.LogInformation($"PK: {record.Dynamodb.Keys["PK"].S}");
            context.Logger.LogInformation($"SK: {record.Dynamodb.Keys["SK"].S}");
            // Log the entire new image as JSON to see all its attributes
            context.Logger.LogInformation($"New Image JSON: {JsonSerializer.Serialize(record.Dynamodb.NewImage)}");

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
                    var localDomain = Environment.GetEnvironmentVariable("LOCAL_DOMAIN") ?? "peerspace.online";
                    var username = newImage["Username"].S;
                    var userHandle = username.Contains("@") ? username : $"{username}@{localDomain}";

                    cypherQuery = @"
                        MERGE (u:User { handle: $handle })
                        ON CREATE SET u.createdAt = timestamp()
                        SET u.displayName = $displayName";

                    parameters.Add("handle", userHandle);
                    parameters.Add("displayName", newImage["DisplayName"].S);

                    break;
                case "Follow":
                    var followingValue = sk.Replace("FOLLOWS#", "");

                    string followingUserId;
                    if (followingValue.Contains("@"))
                    {
                        // Already username@domain
                        followingUserId = followingValue;
                    }
                    else if (followingValue.StartsWith("http"))
                    {
                        // Convert actor URL to username@domain
                        followingUserId = ConvertActorUrlToUserId(followingValue);
                    }
                    else
                    {
                        // Local user without domain
                        followingUserId = $"{followingValue}@{Environment.GetEnvironmentVariable("LOCAL_DOMAIN") ?? "peerspace.online"}";
                    }

                    var followingPk = $"USER#{followingUserId}";

                    // PK is already correct format in DynamoDB (USER#username@domain)
                    var followerPk = pk;

                    context.Logger.LogInformation($"Creating follow relationship: {followerPk} -> {followingPk}");

                    cypherQuery = @"
        MERGE (follower:User { pk: $followerPk })
        ON CREATE SET follower.createdAt = timestamp()
        MERGE (following:User { pk: $followingPk })
        ON CREATE SET following.createdAt = timestamp()
        MERGE (follower)-[:FOLLOWS]->(following)
    ";
                    parameters.Add("followerPk", followerPk);
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

    private static string ConvertActorUrlToUserId(string actorUrl)
    {
        try
        {
            var uri = new Uri(actorUrl);
            var segments = uri.AbsolutePath.Trim('/').Split('/');
            var username = segments.Last();
            return $"{username}@{uri.Host}";
        }
        catch
        {
            return actorUrl; // fallback, probably local
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