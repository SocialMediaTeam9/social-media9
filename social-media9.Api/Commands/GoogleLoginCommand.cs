using MediatR;
using social_media9.Api.Models;

namespace social_media9.Api
{
    public class GoogleLoginCommand : IRequest<AuthResponseDto>
    {
        public string Code { get; set; } = string.Empty;
        public string RedirectUri { get; set; } = string.Empty;
    }
}