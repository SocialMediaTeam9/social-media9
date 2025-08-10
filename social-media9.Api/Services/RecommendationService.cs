using Gremlin.Net.Driver;

public class RecommendationService
{
    private readonly GremlinClient _gremlinClient;

    public RecommendationService(GremlinClient gremlinClient)
    {
        _gremlinClient = gremlinClient;
    }

    public async Task<List<string>> GetPeopleYouMayKnowAsync(string username)
    {
        var userPk = $"USER#{username}";

    
        var query = $"g.V('{userPk}').out('FOLLOWS').as('followed').out('FOLLOWS').where(neq('followed')).where(values('username')).dedup().limit(10).values('username')";

        var result = await _gremlinClient.SubmitAsync<string>(query);
        return result.ToList();
    }
}