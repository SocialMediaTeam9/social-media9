using System.Text.Json.Serialization;

namespace social_media9.Api.Models;
public record GtsUserInfoRequest([property: JsonPropertyName("username")] string Username);

public record GtsUserInfoResponse(
    [property: JsonPropertyName("username")] string Username,
    [property: JsonPropertyName("display_name")] string DisplayName,
    [property: JsonPropertyName("public_key")] string PublicKey,
    [property: JsonPropertyName("private_key")] string PrivateKey,
    [property: JsonPropertyName("actor_type")] string ActorType = "Person"
);

public record GtsCollectionRequest(
    [property: JsonPropertyName("username")] string Username
);

public record GtsCollectionResponse(
    [property: JsonPropertyName("items")] List<string> Items
);

public record GenerateUploadUrlRequest(
    string FileName,
    string ContentType
);

public record GenerateUploadUrlResponse(
   
    string UploadUrl,
    string FinalUrl
);

// public record CreatePostRequest(
//     string Content,
//     List<string>? AttachmentUrls
// );

// public record UserSummary(string Username, string ActorUrl, string? ProfilePictureUrl);

// DTO for creating a new post
// public record CreatePostRequest(string Content);

// DTO for following a user
public record FollowUserRequest(string ActorUrl);

// DTO to represent a post in an API response
public record PostResponse(string PostId, string AuthorUsername, string Content, DateTime CreatedAt, int CommentCount);