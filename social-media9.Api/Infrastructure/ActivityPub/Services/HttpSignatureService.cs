public class HttpSignatureService
{
    public Dictionary<string, string> SignHeaders(string url, string body, string privateKeyPem)
    {
        var headers = new Dictionary<string, string>
        {
            ["Date"] = DateTime.UtcNow.ToString("r"),
            ["Host"] = new Uri(url).Host,
            ["Digest"] = "SHA-256=" + Convert.ToBase64String(System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(body)))
        };

        var stringToSign = "(request-target): post " + new Uri(url).AbsolutePath + "\n" +
                           "host: " + headers["Host"] + "\n" +
                           "date: " + headers["Date"] + "\n" +
                           "digest: " + headers["Digest"];

        var signature = Sign(privateKeyPem, stringToSign);

        var signatureHeader = $"keyId=\"{url}#main-key\",algorithm=\"rsa-sha256\",headers=\"(request-target) host date digest\",signature=\"{signature}\"";

        headers["Signature"] = signatureHeader;
        return headers;
    }

    public string Sign(string privateKeyPem, string stringToSign)
    {
        using var rsa = RSA.Create();
        rsa.ImportFromPem(privateKeyPem);

        var data = Encoding.UTF8.GetBytes(stringToSign);
        var signed = rsa.SignData(data, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

        return Convert.ToBase64String(signed);
    }
}
