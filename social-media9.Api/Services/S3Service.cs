using Amazon.S3;
using Amazon.S3.Model;
using social_media9.Api.Models;

public class S3Service(IAmazonS3 s3Client, IConfiguration config)
{
    private readonly IAmazonS3 _s3Client = s3Client;
    private readonly string _bucketName = config["Aws:S3BucketName"];
    private readonly string _cloudfrontDomain = config["Aws:CloudFrontDomain"];

    /// <summary>
    /// Generates a pre-signed URL for securely uploading a file to S3.
    /// </summary>
    /// <param name="userId">The ID of the user uploading the file, for namespacing.</param>
    /// <param name="fileName">The original name of the file.</param>
    /// <param name="contentType">The MIME type of the file (e.g., "image/jpeg").</param>
    /// <returns>A response containing the upload URL and the final access URL.</returns>
    public GenerateUploadUrlResponse GetPresignedUploadUrl(string userId, string fileName, string contentType)
    {
        var objectKey = $"uploads/{userId}/{Guid.NewGuid()}-{fileName}";

        var request = new GetPreSignedUrlRequest
        {
            BucketName = _bucketName,
            Key = objectKey,
            Verb = HttpVerb.PUT,
            ContentType = contentType,
            Expires = DateTime.UtcNow.AddMinutes(10)
        };

        string uploadUrl = _s3Client.GetPreSignedURL(request);
        string finalUrl = $"https://{_cloudfrontDomain}/{objectKey}";

        return new GenerateUploadUrlResponse(uploadUrl, finalUrl);
    }
}