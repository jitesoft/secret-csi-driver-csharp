using YamlDotNet.Serialization;

namespace CsiSecretProvider.GrpcServices;

[YamlSerializable]
public record SecretReference
{
    public required string Key { get; set; }
    public required string Type { get; set; }
    public string? Format { get; set; } = null;
    public string? PublicKey { get; set; } = null;
    public string? PrivateKey { get; set; } = null;
    public string? ObjectName { get; set; } = null;
}
