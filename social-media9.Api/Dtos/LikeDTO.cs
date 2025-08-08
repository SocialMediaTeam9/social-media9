namespace social_media9.Api.DTOs
{
    public record LikePostRequest(string PostId);
    
    public record UnlikePostRequest(string PostId);
    
    public record LikeResponse(string LikeId, string PostId, string UserId, DateTime CreatedAt);
    
    public record PostLikesResponse(string PostId, int LikeCount, bool IsLikedByUser, List<LikeResponse> Likes);
}