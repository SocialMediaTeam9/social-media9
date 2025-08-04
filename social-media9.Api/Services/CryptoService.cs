using System.Security.Cryptography;

namespace social_media9.Api.Services;

public interface ICryptoService
{
    (string PublicKey, string PrivateKey) GenerateRsaKeyPair();
}

public class CryptoService : ICryptoService
{
    /// <summary>
    /// Generates a new 2048-bit RSA key pair.
    /// </summary>
    /// <returns>A tuple containing the PEM-encoded public and private keys.</returns>
    public (string PublicKey, string PrivateKey) GenerateRsaKeyPair()
    {
        
        using var rsa = RSA.Create(2048);

       
        string publicKeyPem = rsa.ExportRSAPublicKeyPem();

        string privateKeyPem = rsa.ExportPkcs8PrivateKeyPem();

        return (publicKeyPem, privateKeyPem);
    }

    //  public async Task<bool> RegisterNewUserAsync(string username, string displayName, string email, string password)
    // {
    
    //     string hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);
        
    //     (string publicKey, string privateKey) = _cryptoService.GenerateRsaKeyPair();

    //     var newUser = UserEntity.Create(
    //         userId: Ulid.NewUlid().ToString(),
    //         username: username,
    //         displayName: displayName,
    //         email: email,
    //         hashedPassword: hashedPassword,
    //         publicKey: publicKey,
    //         privateKey: privateKey
    //     );

        
    //     bool success = await _dbService.CreateUserAsync(newUser);

    //     return success;
    // }
}