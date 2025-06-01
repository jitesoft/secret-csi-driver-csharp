using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace CsiSecretProvider.Services;

public class BogusSecretManager : ISecretManager
{
    public string? GetSecret(string key, CancellationToken token = default)
    {
        return $"{key}-secret-{DateTimeOffset.Now.ToUnixTimeSeconds()}";
    }

    public X509Certificate2? GetCertificate(string key, CancellationToken token = default)
    {
        using var rsa = RSA.Create();
        var request = new CertificateRequest(
            "cn=test",
            rsa,
            HashAlgorithmName.SHA256,
            RSASignaturePadding.Pkcs1
        );

        return request.CreateSelfSigned(DateTimeOffset.Now, DateTimeOffset.Now.AddHours(9));
    }
}
