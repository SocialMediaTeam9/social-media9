using Neo4j.Driver;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class RecommendationService : IAsyncDisposable
{
    private readonly IDriver _driver;

    public RecommendationService(IConfiguration config)
    {
        var uri = config["Neo4j:Uri"]!;
        var user = config["Neo4j:Username"]!;
        var password = config["Neo4j:Password"]!;
        _driver = GraphDatabase.Driver(uri, AuthTokens.Basic(user, password));
    }

    public async Task<List<string>> GetPeopleYouMayKnowAsync(string username)
    {
        await using var session = _driver.AsyncSession();

        var currentUserHandle = $"{username}@{"peerspace.online"}";
        var currentUserPk = $"USER#{username}@peerspace.online";
        var currentUserPkWithoutHandle = $"USER#{username}";

        var query = @"
            MATCH (me:User)
            WHERE me.pk = $currentUserPk or me.pk = $currentUserPkWithoutHandle OR me.username = $username
            
            MATCH (me)-[:FOLLOWS]->(friend:User)-[:FOLLOWS]->(recommendation:User)
            
            WHERE me <> recommendation AND NOT (me)-[:FOLLOWS]->(recommendation)

            
            RETURN DISTINCT COALESCE(recommendation.handle, recommendation.username, recommendation.pk) AS recommendedIdentifier
            LIMIT 10";
        var result = await session.ExecuteReadAsync(async tx =>
        {
            // 3. Pass all possible identifiers as parameters to the query.
            var parameters = new
            {
                currentUserHandle,
                currentUserPk,
                currentUserPkWithoutHandle,
                username
            };

            var cursor = await tx.RunAsync(query, parameters);

            // 4. The result will be a list of the best available identifiers for the recommended users.
            return await cursor.ToListAsync(record => record["recommendedIdentifier"].As<string>().Replace("USER#", ""));
        });

        return result;
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