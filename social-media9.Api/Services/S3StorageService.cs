using Amazon.S3;
using Amazon.S3.Transfer;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Threading.Tasks;

namespace social_media9.Api.Services
{
    public class S3StorageService : IS3StorageService
    {
        private readonly IAmazonS3 _s3Client;
        private readonly string _bucketName;

        public S3StorageService(IAmazonS3 s3Client, IConfiguration configuration)
        {
            _s3Client = s3Client;
            _bucketName = configuration["AWS:S3BucketName"]; // Get bucket name from appsettings.json
        }

        public async Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType)
        {
            var fileTransferUtility = new TransferUtility(_s3Client);
            var key = $"profile-pictures/{fileName}"; // Example path in S3

            await fileTransferUtility.UploadAsync(new TransferUtilityUploadRequest
            {
                InputStream = fileStream,
                BucketName = _bucketName,
                Key = key,
                CannedACL = S3CannedACL.PublicRead, // Make the file publicly accessible
                ContentType = contentType
            });

            // Construct the public URL for the uploaded file
            return $"https://{_bucketName}.s3.amazonaws.com/{key}";
        }

        public async Task<bool> DeleteFileAsync(string fileUrl)
        {
            try
            {
                // Extract the key from the URL (e.g., "profile-pictures/image.jpg")
                var uri = new System.Uri(fileUrl);
                var key = uri.AbsolutePath.TrimStart('/');

                await _s3Client.DeleteObjectAsync(_bucketName, key);
                return true;
            }
            catch
            {
                return false; // Or throw a more specific exception
            }
        }
    }
}