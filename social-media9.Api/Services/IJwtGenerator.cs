namespace social_media9.Api.Services
{
    public interface IJwtGenerator
    {
        string GenerateToken(string userId, string username);
    }
}