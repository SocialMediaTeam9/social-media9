using social_media9.Api.Services;
using System;

namespace social_media9.Api.Services
{
    public interface IPasswordHasher
    {
        string HashPassword(string password);
        bool VerifyPassword(string password, string hashedPassword);
    }
}
