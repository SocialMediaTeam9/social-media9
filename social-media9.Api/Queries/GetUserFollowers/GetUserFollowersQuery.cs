using MediatR;
using social_media9.Api.Models;
using System.Collections.Generic;

namespace social_media9.Api
{
    public class GetUserFollowersQuery : IRequest<IEnumerable<UserSummary>>
    {
        public string Username { get; set; } = string.Empty;
    }
}