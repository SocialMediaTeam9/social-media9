using MediatR;
using social_media9.Api.Dtos;
using social_media9.Api.Models;
using social_media9.Api.Repositories.Interfaces;
using social_media9.Api.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace social_media9.Api.Queries.SearchAll
{
    public class SearchAllQueryHandler : IRequestHandler<SearchAllQuery, IEnumerable<SearchResultDto>>
    {
        private readonly IUserRepository _userRepository;
        private readonly IPostRepository _postRepository;
        private readonly IFederationService _federationService;

        // Inject all necessary services
        public SearchAllQueryHandler(IUserRepository userRepository, IPostRepository postRepository, IFederationService federationService)
        {
            _userRepository = userRepository;
            _postRepository = postRepository;
            _federationService = federationService;
        }

        public async Task<IEnumerable<SearchResultDto>> Handle(SearchAllQuery request, CancellationToken cancellationToken)
        {
            var query = request.Query.Trim();
            var userResults = new List<User>();
            var postResults = new List<Post>();

            // Regex to check if the query is a remote user handle
            var remoteUserRegex = new Regex(@"^@?([\w\.\-]+)@([\w\.\-]+)$");
            var match = remoteUserRegex.Match(query);

            if (match.Success)
            {
                var discoveredUser = await _federationService.DiscoverAndCacheUserAsync(match.Value.TrimStart('@'));
                if (discoveredUser != null)
                {
                    userResults.Add(discoveredUser);
                }
            }
            else
            {
                var userSearchTask = _userRepository.SearchUsersAsync(query, request.Limit);
                var postSearchTask = _postRepository.SearchPostsAsync(query, request.Limit);

                await Task.WhenAll(userSearchTask, postSearchTask);

                userResults.AddRange(await userSearchTask);
                postResults.AddRange(await postSearchTask);
            }


            var combinedResults = new List<SearchResultDto>();

            combinedResults.AddRange(userResults.Select(u => new SearchResultDto
            {
                ResultType = "User", 
                UserId = u.UserId,
                Username = u.IsRemote ? $"{u.Username}@{new Uri(u.ActorUrl!).Host}" : u.Username,
                FullName = u.FullName,
                ProfilePictureUrl = u.ProfilePictureUrl,
                CreatedAt = u.CreatedAt
            }));

            combinedResults.AddRange(postResults.Select(p => new SearchResultDto
            {
                ResultType = "Post",
                PostId = p.SK.Replace("POST#", ""),
                Username = p.AuthorUsername,
                Content = p.Content,
                CreatedAt = p.CreatedAt
            }));

            return combinedResults.OrderByDescending(r => r.CreatedAt).Take(request.Limit);
        }
    }
}