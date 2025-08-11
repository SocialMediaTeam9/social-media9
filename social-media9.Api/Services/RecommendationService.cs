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
        var query = @"
            MATCH (currentUser:User { username: $username })-[:FOLLOWS]->(followed:User)
            MATCH (followed)-[:FOLLOWS]->(recommendation:User)
            WHERE NOT (currentUser)-[:FOLLOWS]->(recommendation) AND currentUser <> recommendation
            RETURN recommendation.username AS recommendedUsername
            LIMIT 10";
        var result = await session.ExecuteReadAsync(async tx => {
            var cursor = await tx.RunAsync(query, new { username });
            return await cursor.ToListAsync(record => record["recommendedUsername"].As<string>());
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