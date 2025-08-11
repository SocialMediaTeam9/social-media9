using MediatR;

namespace social_media9.Api.Queries{
    /// <summary>
    /// A query to check if a specific follow relationship exists.
    /// </summary>
    /// <param name="LocalUsername">The user performing the follow.</param>
    /// <param name="TargetUsername">The user being followed.</param>
    public record IsFollowingQuery(
        string LocalUsername,
        string TargetUsername
    ) : IRequest<bool>;
}