using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System;

// No external dependencies needed for this class beyond the .NET framework.
public class ActivityPubService
{
    private readonly HttpClient _httpClient;
    private readonly string _keyId;
    private readonly RSA _privateKey;

    /// <summary>
    /// Creates a service to deliver a signed activity on behalf of a single user.
    /// </summary>
    /// <param name="httpClient">An HttpClient for making the request.</param>
    /// <param name="actorUrl">The full Actor URL of the signing user (e.g., https://peerspace.online/users/alice).</param>
    /// <param name="privateKeyPem">The user's PEM-encoded private key.</param>
    public ActivityPubService(HttpClient httpClient, string actorUrl, string privateKeyPem)
    {
        _httpClient = httpClient;
        _keyId = $"{actorUrl}#main-key";
        
        _privateKey = RSA.Create();
        _privateKey.ImportFromPem(privateKeyPem.ToCharArray());
    }

    /// <summary>
    /// Signs and delivers a JSON activity to a target inbox URL.
    /// </summary>
    public async Task<HttpResponseMessage> DeliverActivityAsync(string targetInboxUrl, JsonDocument activity)
    {
        string body = JsonSerializer.Serialize(activity.RootElement);
        string digest = ComputeDigestHeader(body);

        var request = new HttpRequestMessage(HttpMethod.Post, targetInboxUrl)
        {
            Content = new StringContent(body, Encoding.UTF8, "application/activity+json")
        };

        request.Headers.Date = DateTimeOffset.UtcNow;
        request.Headers.TryAddWithoutValidation("Digest", digest);
        request.Headers.Host = new Uri(targetInboxUrl).Host;

        string signatureHeaderValue = BuildSignatureHeader(request, digest);
        request.Headers.Authorization = new AuthenticationHeaderValue("Signature", signatureHeaderValue);
        
        return await _httpClient.SendAsync(request);
    }

    private string ComputeDigestHeader(string body)
    {
        using var sha256 = SHA256.Create();
        var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(body));
        return "SHA-256=" + Convert.ToBase64String(hash);
    }

    private string BuildSignatureHeader(HttpRequestMessage request, string digest)
    {
        var signingString = new StringBuilder();
        signingString.Append("(request-target): post " + request.RequestUri.AbsolutePath + "\n");
        signingString.Append("host: " + request.Headers.Host + "\n");
        signingString.Append("date: " + request.Headers.Date?.ToString("R") + "\n");
        signingString.Append("digest: " + digest);

        var signatureBytes = _privateKey.SignData(
            Encoding.UTF8.GetBytes(signingString.ToString()),
            HashAlgorithmName.SHA256,
            RSASignaturePadding.Pkcs1
        );

        string signature = Convert.ToBase64String(signatureBytes);

        return $"keyId=\"{_keyId}\",algorithm=\"rsa-sha256\",headers=\"(request-target) host date digest\",signature=\"{signature}\"";
    }
}