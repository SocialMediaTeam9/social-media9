using Gremlin.Net.Driver;
using Gremlin.Net.Structure.IO.GraphSON;

public class RecommendationService
{
    private readonly GremlinClient _gremlinClient;
    private readonly ILogger<RecommendationService> _logger;


    public RecommendationService(IConfiguration config, ILogger<RecommendationService> logger)
    {

        _logger = logger;
        var endpoint = config["Neptune:Endpoint"] ?? throw new InvalidOperationException("Neptune:Endpoint not configured.");
        var port = int.Parse(config["Neptune:Port"] ?? "8182");

        var gremlinServer = new GremlinServer(endpoint, port, enableSsl: true);

        _gremlinClient = new GremlinClient(gremlinServer, new GraphSON2Reader(), new GraphSON2Writer());
    }

    public async Task<List<string>> GetPeopleYouMayKnowAsync(string username)
    {
        var userPk = $"USER#{username}";


        var query = $"g.V('{userPk}').out('FOLLOWS').as('followed').out('FOLLOWS').where(neq('followed')).where(values('username')).dedup().limit(10).values('username')";

        var result = await _gremlinClient.SubmitAsync<string>(query);
        return result.ToList();
    }
}