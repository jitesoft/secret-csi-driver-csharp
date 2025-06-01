using CsiSecretProvider.GrpcServices;
using CsiSecretProvider.Services;
using Microsoft.AspNetCore.Server.Kestrel.Core;

var builder = WebApplication.CreateBuilder(args);

// Make sure the endpoint socket doesn't exist, if so, remove and re-create.
var endpoint = builder.Configuration.GetValue<string>("Endpoint") ?? throw new ArgumentException("Endpoint not defined");
if (File.Exists(endpoint))
{
    Console.WriteLine("Removing stale socket at {0}", endpoint);
    File.Delete(endpoint);
}

builder.Services.AddSingleton<ISecretManager, BogusSecretManager>();

builder.WebHost.ConfigureKestrel(o =>
{
    o.ListenUnixSocket(endpoint, opt =>
    {
        opt.Protocols = HttpProtocols.Http2;
    });

    o.AllowHostHeaderOverride = true;
    o.AddServerHeader = false;
    o.AllowAlternateSchemes = true;
});

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.AllowedHosts.Add("*");
    options.AllowedHosts.Add("/etc/kubernetes/secrets-store-csi-providers/jitesoft.sock");
});

builder.Services.AddGrpc(o => { });
var app = builder.Build();
app.MapGrpcService<SecretProviderService>();

await app.RunAsync();
