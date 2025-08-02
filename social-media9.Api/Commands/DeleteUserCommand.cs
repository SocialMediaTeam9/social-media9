using MediatR;
using social_media9.Api.Models;

namespace social_media9.Api
{
    public class DeleteUserCommand : IRequest<Unit> // Unit is MediatR's void equivalent
    {
        public string UserId { get; set; } = string.Empty; // Changed to string
    }
}