using System.Text.Json.Serialization;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace CsiSecretProvider.GrpcServices;

public record MountRequestAttributes
{
    [JsonPropertyName("csi.storage.k8s.io/ephemeral")]
    public string Ephemeral { get; set; } = null!;

    [JsonPropertyName("csi.storage.k8s.io/pod.name")]
    public string PodName { get; set; } = null!;

    [JsonPropertyName("csi.storage.k8s.io/pod.namespace")]
    public string PodNamespace { get; set; } = null!;

    [JsonPropertyName("csi.storage.k8s.io/pod.uid")]
    public string PodUid { get; set; } = null!;

    [JsonPropertyName("csi.storage.k8s.io/serviceAccount.name")]
    public string ServiceAccount { get; set; } = null!;

    [JsonPropertyName("secretProviderClass")]
    public string SecretProviderClass { get; set; } = null!;

    [JsonPropertyName("objects")]
    public string Objects { private get; set; } = null!;

    [JsonIgnore]
    public IList<SecretReference> Secrets => new DeserializerBuilder()
        .WithNamingConvention(CamelCaseNamingConvention.Instance).Build().Deserialize<IList<SecretReference>>(Objects);

}

[JsonSerializable(typeof(MountRequestAttributes))]
internal partial class MountRequestAttributesSerializerContext : JsonSerializerContext {}
