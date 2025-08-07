using MediatR;
using social_media9.Api.Dtos;
using social_media9.Api.Repositories.Interfaces;
using social_media9.Api.Services.Interfaces; // New using for our service
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using social_media9.Api.Models;

namespace social_media9.Api.Queries.SearchUsers
{
    public class SearchUsersQueryHandler : IRequestHandler<SearchUsersQuery, IEnumerable<UserSearchResultDto>>
    {
        private readonly IUserRepository _userRepository;
        private readonly IFederationService _federationService;

        public SearchUsersQueryHandler(IUserRepository userRepository, IFederationService federationService)
        {
            _userRepository = userRepository;
            _federationService = federationService;
        }

        public async Task<IEnumerable<UserSearchResultDto>> Handle(SearchUsersQuery request, CancellationToken cancellationToken)
        {
            var query = request.Query.Trim();
            var results = new List<User>();

            var remoteUserRegex = new Regex(@"^@?([\w\.\-]+)@([\w\.\-]+)$");
            var match = remoteUserRegex.Match(query);

            if (match.Success)
            {

                var discoveredUser = await _federationService.DiscoverAndCacheUserAsync(match.Value.TrimStart('@'));
                if (discoveredUser != null)
                {
                    results.Add(discoveredUser);
                }
            }
            else
            {
                // This is a local search. Query our database as before.
                var localResults = await _userRepository.SearchUsersAsync(query, request.Limit);
                results.AddRange(localResults);
            }

            return results.Select(user =>
            {
                string displayUsername = user.Username;

                if (user.IsRemote && user.ActorUrl != null)
                {
                    var uri = new Uri(user.ActorUrl);
                    displayUsername = $"{user.Username}@{uri.Host}";
                }

                return new UserSearchResultDto
                {
                    UserId = user.UserId,
                    Username = displayUsername,
                    FullName = user.FullName,
                    ProfilePictureUrl = user.ProfilePictureUrl
                };
            });
        }
    }
}