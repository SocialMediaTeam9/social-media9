using System.IO;
using System.Threading.Tasks;

namespace social_media9.Api.Services
{
    public interface IS3StorageService
    {
        Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType);
        Task<bool> DeleteFileAsync(string fileUrl);
        // You might add methods for getting pre-signed URLs if you want to upload directly from frontend to S3
    }
}