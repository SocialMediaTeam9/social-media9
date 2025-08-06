using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

public class HttpSignatureValidationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<HttpSignatureValidationMiddleware> _logger;

    public HttpSignatureValidationMiddleware(RequestDelegate next, ILogger<HttpSignatureValidationMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (!context.Request.Path.StartsWithSegments("/inbox") || context.Request.Method != "POST")
        {
            await _next(context);
            return;
        }

        var headers = context.Request.Headers;

        if (!headers.ContainsKey("Signature") || !headers.ContainsKey("Date") || !headers.ContainsKey("Digest"))
        {
            context.Response.StatusCode = 400;
            await context.Response.WriteAsync("Missing required HTTP Signature headers.");
            return;
        }

        try
        {
            var signatureHeader = ParseSignatureHeader(headers["Signature"]!);
            var signingString = BuildSigningString(signatureHeader["headers"], context);

            var keyId = signatureHeader["keyId"];
            var signature = Convert.FromBase64String(signatureHeader["signature"]);

            var actorUrl = keyId.Split('#')[0];
            using var http = new HttpClient();
            var actorJson = await http.GetStringAsync(actorUrl);
            var doc = JsonDocument.Parse(actorJson);
            var publicKeyPem = doc.RootElement
                .GetProperty("publicKey")
                .GetProperty("publicKeyPem")
                .GetString();

            var rsa = LoadRsaPublicKey(publicKeyPem);
            var valid = rsa.VerifyData(
                Encoding.UTF8.GetBytes(signingString),
                signature,
                HashAlgorithmName.SHA256,
                RSASignaturePadding.Pkcs1
            );

            if (!valid)
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("Invalid HTTP Signature.");
                return;
            }

            // Signature valid, continue
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating HTTP Signature");
            context.Response.StatusCode = 400;
            await context.Response.WriteAsync("Error validating signature.");
        }
    }

    private static Dictionary<string, string> ParseSignatureHeader(string header)
    {
        return header.Split(',')
            .Select(part => part.Split('=')).ToDictionary(
                kv => kv[0].Trim(),
                kv => kv[1].Trim('"')
            );
    }

    private static string BuildSigningString(string headersList, HttpContext context)
    {
        var headers = headersList.Split(' ');
        var sb = new StringBuilder();

        foreach (var header in headers)
        {
            if (header == "(request-target)")
            {
                sb.Append("(request-target): post ")
                  .Append(context.Request.Path)
                  .Append('\n');
            }
            else
            {
                sb.Append(header.ToLower()).Append(": ")
                  .Append(context.Request.Headers[header])
                  .Append('\n');
            }
        }

        return sb.ToString().TrimEnd('\n');
    }

    private static RSA LoadRsaPublicKey(string pem)
    {
        var rsa = RSA.Create();
        rsa.ImportFromPem(pem.ToCharArray());
        return rsa;
    }
}