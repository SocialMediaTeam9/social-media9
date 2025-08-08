using social_media9.Api.Services.DynamoDB;


public interface ITimelineService
{
    Task<PaginatedTimelineResponse> GetUserTimelineAsync(string username, int pageSize, string? cursor);
    Task<PaginatedTimelineResponse> GetPublicTimelineAsync(int pageSize, string? cursor);
}

public class TimelineService : ITimelineService
{
    private readonly DynamoDbService _dbService;

    public TimelineService(DynamoDbService dbService)
    {
        _dbService = dbService;
    }

    public async Task<PaginatedTimelineResponse> GetUserTimelineAsync(string username, int pageSize, string? cursor)
    {
        var (dbItems, nextToken) = await _dbService.GetTimelineAsync(username, pageSize, cursor);

        var responseItems = dbItems.Select(item => new TimelineItemResponse(
            PostId: item.SK.Replace("NOTE#", ""),
            AuthorUsername: item.AuthorUsername,
            PostContent: item.PostContent,
            AttachmentUrls: item.AttachmentUrls,
            CreatedAt: item.CreatedAt,
            BoostedBy: item.BoostedBy
        )).ToList();

        return new PaginatedTimelineResponse(responseItems, nextToken);
    }

    public async Task<PaginatedTimelineResponse> GetPublicTimelineAsync(int pageSize, string? cursor)
    {
        // The only difference is we pass "PUBLIC" as the username to query the public partition
        var (dbItems, nextToken) = await _dbService.GetTimelineAsync("PUBLIC", pageSize, cursor);

        var responseItems = dbItems.Select(item => new TimelineItemResponse(
            PostId: item.SK.Replace("NOTE#", "").Split('#').Last(), // Safely get PostId from the new SK format
            AuthorUsername: item.AuthorUsername,
            PostContent: item.PostContent,
            AttachmentUrls: item.AttachmentUrls,
            CreatedAt: item.CreatedAt,
            BoostedBy: item.BoostedBy
        )).ToList();

        return new PaginatedTimelineResponse(responseItems, nextToken);
    }
}