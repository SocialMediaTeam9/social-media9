public record UnfollowUserRequest(string ActorUrl);


public record FollowDTO(string localUsername, string targetUsername);