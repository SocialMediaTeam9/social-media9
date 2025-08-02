using MediatR;
using social_media9.Api.Models;
using System.Collections.Generic;

namespace social_media9.Api
{
    public class GetUserFollowingQuery : IRequest<IEnumerable<UserSummary>>
    {
        public string UserId { get; set; } = string.Empty; // Changed to string
    }
}