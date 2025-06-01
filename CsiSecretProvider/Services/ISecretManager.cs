using System.Security.Cryptography.X509Certificates;

namespace CsiSecretProvider.Services;

public interface ISecretManager
{
    public string? GetSecret(string key, CancellationToken token = default);
    public X509Certificate2? GetCertificate(string key, CancellationToken token = default);
}
