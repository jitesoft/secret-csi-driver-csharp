using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using CsiSecretProvider.Services;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Grpc.Core;
using V1Alpha1;
using File = V1Alpha1.File;

namespace CsiSecretProvider.GrpcServices;

public class SecretProviderService : CSIDriverProvider.CSIDriverProviderBase
{
    private readonly ILogger<SecretProviderService> _logger;
    private readonly ISecretManager _secretManager;

    public SecretProviderService(ILogger<SecretProviderService> logger, ISecretManager secretManager)
    {
        _logger = logger;
        _secretManager = secretManager;
    }

    public override Task<VersionResponse> Version(VersionRequest request, ServerCallContext context)
    {
        _logger.LogInformation("Version request received");
        return Task.FromResult(new VersionResponse
        {
            Version = "v1alpha1",
            RuntimeName = "jitesoft-csi-provider",
            RuntimeVersion = Environment.Version.ToString(),
        });
    }

    private (byte[]? Private, byte[]? Public)  GetCertificateBytes(string type, X509Certificate2 certificate)
    {
        if (type.Equals("pem", StringComparison.InvariantCultureIgnoreCase))
        {
            if (certificate.GetECDsaPrivateKey() is not null)
            {
                return (
                    Encoding.UTF8.GetBytes(certificate.GetECDsaPrivateKey()!.ExportECPrivateKeyPem()),
                    Encoding.UTF8.GetBytes(certificate.GetECDsaPublicKey()!.ExportSubjectPublicKeyInfoPem())
                );
            }

            if (certificate.GetRSAPrivateKey() is not null)
            {
                return (
                    Encoding.UTF8.GetBytes(certificate.GetRSAPrivateKey()!.ExportRSAPrivateKeyPem()),
                    Encoding.UTF8.GetBytes(certificate.GetRSAPublicKey()!.ExportRSAPublicKeyPem())
                );
            }
        }

        return (null, null);
    }

    public override Task<MountResponse> Mount(MountRequest request, ServerCallContext context)
    {
        _logger.LogInformation("Mount request received");

        _logger.LogInformation("Request data: ");
        _logger.LogInformation("  Secrets: {Sec}", request.Secrets);
        _logger.LogInformation("  Attributes: {Attr}", request.Attributes);
        _logger.LogInformation("  Permission: {Perm}", request.Permission);
        _logger.LogInformation("  TargetPath: {Path}", request.TargetPath);

        var objects = JsonSerializer.Deserialize<MountRequestAttributes>(request.Attributes);

        var files = new RepeatedField<File>();
        foreach (var obj in objects!.Secrets)
        {
            _logger.LogDebug("Secret:\n\t{SecretJson}", JsonSerializer.Serialize(obj));

            // Type.
            var isSecret = obj.Type.Equals("secret", StringComparison.InvariantCultureIgnoreCase);
            if (isSecret)
            {
                var secret = _secretManager.GetSecret(obj.Key);
                files.Add(new File
                {
                    Path = obj.ObjectName,
                    Contents = ByteString.CopyFromUtf8(secret),
                });
            }
            else
            {
                using var cert = _secretManager.GetCertificate(obj.Key);
                if (cert is null)
                {
                    _logger.LogError("Failed to mount certificate with key {Key}, certificate was null", obj.Key);
                    continue;
                }

                var (privateKey, publicKey) = GetCertificateBytes("pem", cert);

                if (obj.ObjectName is not null)
                {
                    files.Add(new File
                    {
                        Path = obj.ObjectName,
                        Contents = ByteString.CopyFrom(privateKey),
                    });
                }

                if (obj.PrivateKey is not null)
                {
                    files.Add(new File
                    {
                        Path = obj.PrivateKey,
                        Contents = ByteString.CopyFrom(privateKey),
                    });
                }

                if (obj.PublicKey is not null)
                {
                    files.Add(new File
                    {
                        Path = obj.PublicKey,
                        Contents = ByteString.CopyFrom(publicKey),
                    });
                }
            }
        }

        _logger.LogInformation("Mounting files");

        var response = new MountResponse()
        {
            ObjectVersion = { new ObjectVersion()
            {
                Version = "v1"
            } }
        };
        response.Files.AddRange(files);

        _logger.LogInformation("Done");
        return Task.FromResult(response);
    }
}
