using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using social_media9.Api.Services.Interfaces;

namespace social_media9.Api.Services.Implementations
{
    public class StorageService : IStorageService
    {
        public async Task<string> UploadFileAsync(IFormFile file)
        {
            // TODO: Implement actual storage logic (e.g., AWS S3, local, etc.)
            // For now, just return a dummy URL
            await Task.CompletedTask;
            return $"https://dummy-storage.local/{file.FileName}";
        }
    }
}
