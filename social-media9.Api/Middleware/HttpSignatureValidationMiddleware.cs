// using social_media9.Api.Services.DynamoDB;
// using HttpSignatures;
// using Microsoft.IdentityModel.Tokens;

// public class HttpSignatureValidationMiddleware
// {
//     private readonly RequestDelegate _next;
//     private readonly ILogger<HttpSignatureValidationMiddleware> _logger;
//     private readonly DynamoDbService _dbService; 

//     public HttpSignatureValidationMiddleware(RequestDelegate next, ILogger<HttpSignatureValidationMiddleware> logger, DynamoDbService dbService)
//     {
//         _next = next;
//         _logger = logger;
//         _dbService = dbService;
//     }

//     public async Task InvokeAsync(HttpContext context)
//     {
//         // You would add logic here to only run this on specific paths, like "/users/{username}/inbox"
        
//         var signatureParser = new SignatureParser();
//         System.Security.Cryptography.Xml.Signature signature;
//         try
//         {
//             signature = signatureParser.Parse(context.Request);
//         }
//         catch (Exception ex)
//         {
//             _logger.LogWarning(ex, "Failed to parse HTTP Signature");
//             context.Response.StatusCode = 401;
//             return;
//         }

//         // The keyId is the Actor URL that signed the request
//         var keyId = signature.KeyId;
//         var signingActorUsername = keyId.Split('/').Last(); // Simplistic way to get username

//         // Fetch the actor's public key from your database (or cache)
//         var actor = await _dbService.GetUserProfileByUsernameAsync(signingActorUsername);
//         if (actor == null)
//         {
//              // In a real app, you'd fetch the remote actor's profile from their server here.
//              _logger.LogWarning("Could not find signing actor {ActorUsername}", signingActorUsername);
//              context.Response.StatusCode = 401;
//              return;
//         }

//         var signatureValidator = new SignatureValidator(new RsaSha256SignatureAlgorithm(actor.PublicKeyPem));
//         if (!signatureValidator.IsValid(signature))
//         {
//             _logger.LogWarning("Invalid HTTP Signature for actor {ActorUsername}", signingActorUsername);
//             context.Response.StatusCode = 401;
//             return;
//         }
        
//         // If the signature is valid, proceed to the controller action.
//         _logger.LogInformation("Successfully validated HTTP Signature for {ActorUsername}", signingActorUsername);
//         await _next(context);
//     }
// }