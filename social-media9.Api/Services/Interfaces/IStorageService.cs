using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace social_media9.Api.Services.Interfaces
{
    public interface IStorageService
    {
        Task<string> UploadFileAsync(IFormFile file);
    }
}
