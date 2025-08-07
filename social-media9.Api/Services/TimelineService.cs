using social_media9.Api.Services.DynamoDB;
using social_media9.Api.Models.DynamoDb;
using social_media9.Api.Models;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;


public interface ITimelineService
{
    Task<PaginatedTimelineResponse> GetUserTimelineAsync(string username, int pageSize, string? cursor);
    Task ProcessIncomingPostAsync(string inboxOwner, string content, string actorUri);
    Task<PaginatedTimelineResponse> GetTimelineAsync(string username, string? cursor);
}

public class TimelineService : ITimelineService
{
    private readonly DynamoDbService _dbService;
    private readonly IDynamoDBContext _context;

    public TimelineService(DynamoDbService dbService, IDynamoDBContext context)
    {
        _dbService = dbService;
        _context = context;
    }

    // public async Task<PaginatedTimelineResponse> GetUserTimelineAsync(string username, int pageSize, string? cursor)
    // {
    //     var (dbItems, nextToken) = await _dbService.GetTimelineAsync(username, pageSize, cursor);

    //     var responseItems = dbItems.Select(item => new TimelineItemResponse(
    //         PostId: item.SK.Replace("NOTE#", ""),
    //         AuthorUsername: item.AuthorUsername,
    //         PostContent: item.PostContent,
    //         AttachmentUrls: item.AttachmentUrls,
    //         CreatedAt: item.CreatedAt,
    //         BoostedBy: item.BoostedBy
    //     )).ToList();

    //     return new PaginatedTimelineResponse(responseItems, nextToken);
    // }

    public async Task<PaginatedTimelineResponse> GetUserTimelineAsync(string username, int pageSize, string? cursor)
    {
        var (dbItems, nextToken) = await _dbService.GetTimelineAsync(username, pageSize, cursor);

        var responseItems = dbItems.Select(item => new TimelineItemResponse(
            PostId: item.PostId,
            AuthorUsername: item.AuthorUsername,
            PostContent: item.PostContent,
            AttachmentUrls: item.AttachmentUrls,
            CreatedAt: item.CreatedAt,
            BoostedBy: item.BoostedBy
        )).ToList();

        return new PaginatedTimelineResponse(responseItems, nextToken);
    }

    public async Task<PaginatedTimelineResponse> GetTimelineAsync(string username, string? cursor)
{
    var config = new DynamoDBOperationConfig
    {
        IndexName = "GSI1",
        BackwardQuery = true,
        // PaginationToken = cursor
    };

    var query = _context.QueryAsync<TimelineItemEntity>(username, config);

    var items = await query.GetNextSetAsync();
    var nextCursor = query.PaginationToken;

    return new PaginatedTimelineResponse(
        Items: items.Select(item => new TimelineItemResponse(
            PostId: item.PostId,
            AuthorUsername: item.AuthorUsername,
            PostContent: item.PostContent,
            AttachmentUrls: item.AttachmentUrls,
            CreatedAt: item.CreatedAt,
            BoostedBy: item.BoostedBy
        )).ToList(),
        NextCursor: nextCursor
    );
}

    public async Task ProcessIncomingPostAsync(string inboxOwner, string content, string actorUri)
    {
        var authorUsername = actorUri.Split('/').Last();

        var entity = new TimelineItemEntity
        {
            PK = inboxOwner,
            SK = $"TIMELINE#{Guid.NewGuid()}",
            PostId = Guid.NewGuid().ToString(),
            AuthorUsername = authorUsername,
            PostContent = content,
            CreatedAt = DateTime.UtcNow
        };

        await _context.SaveAsync(entity);
    }
}