using Neo4j.Driver;
using social_media9.Api.Models;
using System.Threading.Tasks;

public class Neo4jService : IAsyncDisposable
{
    private readonly IDriver _driver;
    private readonly ILogger<Neo4jService> _logger;
    private readonly string _localDomain;

    public Neo4jService(IConfiguration config, ILogger<Neo4jService> logger)
    {
        var uri = config["Neo4j:Uri"]!;
        var user = config["Neo4j:Username"]!;
        var password = config["Neo4j:Password"]!;
        _driver = GraphDatabase.Driver(uri, AuthTokens.Basic(user, password));
        _logger = logger;
        _localDomain = config["DomainName"] ?? "peerspace.online";
    }

    /// <summary>
    /// Ensures a user node exists and creates a FOLLOWS relationship in Neo4j.
    /// This is the primary method for handling follow actions directly from the API.
    /// </summary>
    public async Task CreateFollowRelationshipAsync(UserSummary follower, UserSummary following)
    {
        await using var session = _driver.AsyncSession();

        // The handles are the unique identifiers in our graph.
        var followerHandle = follower.Username.Contains("@") ? follower.Username : $"{follower.Username}@{_localDomain}";
        var followingHandle = following.Username.Contains("@") ? following.Username : $"{following.Username}@{_localDomain}";

        // This single, robust Cypher query does everything:
        // 1. MERGE ensures the follower node exists.
        // 2. MERGE ensures the following node exists.
        // 3. MERGE ensures the relationship is created between them.
        var query = @"
            MERGE (follower:User { handle: $followerHandle })
            MERGE (following:User { handle: $followingHandle })
            MERGE (follower)-[r:FOLLOWS]->(following)
            ON CREATE SET r.createdAt = timestamp()";

        try
        {
            await session.ExecuteWriteAsync(async tx => 
                await tx.RunAsync(query, new { followerHandle, followingHandle })
            );
            _logger.LogInformation("Successfully created FOLLOWS relationship in Neo4j for {Follower} -> {Following}", followerHandle, followingHandle);
        }
        catch (Exception ex)
        {
            // If this fails, we log it, but we don't fail the user's request.
            // The Lambda will eventually sync it as a fallback.
            _logger.LogError(ex, "Failed to create immediate FOLLOWS relationship in Neo4j for {Follower} -> {Following}", followerHandle, followingHandle);
        }
    }
    
    // You would add a similar 'DeleteFollowRelationshipAsync' method here.

    public async ValueTask DisposeAsync() => await _driver.CloseAsync();
}