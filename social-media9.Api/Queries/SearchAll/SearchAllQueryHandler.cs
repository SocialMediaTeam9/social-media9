// <<< THIS IS A NEW FILE >>>
using MediatR;
using social_media9.Api.Dtos;
using social_media9.Api.Repositories.Interfaces;
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

        public SearchAllQueryHandler(IUserRepository userRepository, IPostRepository postRepository)
        {
            _userRepository = userRepository;
            _postRepository = postRepository;
        }

        public async Task<IEnumerable<SearchResultDto>> Handle(SearchAllQuery request, CancellationToken cancellationToken)
        {
            // Start both searches in parallel for efficiency
            var userSearchTask = _userRepository.SearchUsersAsync(request.Query, request.Limit);
            var postSearchTask = _postRepository.SearchPostsAsync(request.Query, request.Limit);

            await Task.WhenAll(userSearchTask, postSearchTask);

            var userResults = await userSearchTask;
            var postResults = await postSearchTask;

            var combinedResults = new List<SearchResultDto>();

            // Map user results
            combinedResults.AddRange(userResults.Select(u => new SearchResultDto
            {
                ResultType = "User",
                UserId = u.UserId,
                Username = u.Username,
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