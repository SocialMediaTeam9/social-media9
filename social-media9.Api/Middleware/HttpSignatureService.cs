// using HttpSignatures;
// using Microsoft.Extensions.Logging;
// using System;
// using System.Linq;
// using System.Net.Http;
// using System.Net.Http.Headers;
// using System.Text.Json;
// using System.Threading.Tasks;

// public interface IHttpSignatureService
// {
//     Task<bool> IsValid(HttpRequest request);
// }

// public class HttpSignatureService : IHttpSignatureService
// {
//     private readonly ILogger<HttpSignatureService> _logger;
//     private readonly DynamoDbService _dbService;
//     private readonly IHttpClientFactory _httpClientFactory;
//     private readonly ISignatureAlgorithm _algorithm;
//     private readonly HttpSignatureHandler _handler;

//     public HttpSignatureService(ILogger<HttpSignatureService> logger, DynamoDbService dbService, IHttpClientFactory httpClientFactory)
//     {
//         _logger = logger;
//         _dbService = dbService;
//         _httpClientFactory = httpClientFactory;
        
//         _algorithm = new RsaSha256SignatureAlgorithm();
        
//         // Configure the handler with a key store that knows how to fetch keys
//         _handler = new HttpSignatureHandler(new KeyStore(GetKey), _algorithm);
//     }

//     /// <summary>
//     /// This is the main validation method.
//     /// </summary>
//     public async Task<bool> IsValid(HttpRequest request)
//     {
//         try
//         {
//             return await _handler.IsSignatureValid(request);
//         }
//         catch (Exception ex)
//         {
//             _logger.LogError(ex, "An exception occurred during signature validation.");
//             return false;
//         }
//     }

//     /// <summary>
//     /// This is the "Key Resolver". The HttpSignatureHandler will call this method
//     /// automatically when it needs to get the public key for a given KeyId.
//     /// </summary>
//     private async Task<IKey> GetKey(string keyId)
//     {
//         _logger.LogInformation("Attempting to resolve public key for KeyId: {KeyId}", keyId);
//         var username = keyId.Split('/').LastOrDefault()?.Split('#').FirstOrDefault();

//         if (string.IsNullOrEmpty(username))
//         {
//             _logger.LogWarning("Could not parse username from KeyId: {KeyId}", keyId);
//             throw new KeyNotFoundException($"Invalid KeyId format: {keyId}");
//         }

//         // First, check if the actor is one of our local users.
//         var localActor = await _dbService.GetUserProfileByUsernameAsync(username);
//         string? publicKeyPem = localActor?.PublicKeyPem;

//         if (publicKeyPem == null)
//         {
//             // If not a local user, fetch the remote actor's profile.
//             try
//             {
//                 var httpClient = _httpClientFactory.CreateClient("FederationClient");
//                 var request = new HttpRequestMessage(HttpMethod.Get, keyId.Split('#').First());
//                 request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/activity+json"));
//                 var response = await httpClient.SendAsync(request);
//                 response.EnsureSuccessStatusCode();

//                 using var doc = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
//                 publicKeyPem = doc.RootElement
//                                   .GetProperty("publicKey")
//                                   .GetProperty("publicKeyPem")
//                                   .GetString();
//             }
//             catch (Exception ex)
//             {
//                 _logger.LogError(ex, "Failed to fetch or parse remote actor public key for KeyId {KeyId}", keyId);
//                 throw new KeyNotFoundException($"Could not fetch key for {keyId}", ex);
//             }
//         }

//         if (string.IsNullOrEmpty(publicKeyPem))
//         {
//             throw new KeyNotFoundException($"Public key was empty for {keyId}");
//         }

//         // Return the key to the handler.
//         return new RsaKey(publicKeyPem);
//     }
// }