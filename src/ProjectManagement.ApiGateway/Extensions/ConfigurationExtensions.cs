using System.Diagnostics.CodeAnalysis;
using Ocelot.DependencyInjection;
using ProjectManagement.ApiGateway.Configuration;

namespace ProjectManagement.ApiGateway.Extensions;

[ExcludeFromCodeCoverage]
public static class ConfigurationExtensions
{
    public static void AddApplicationConfiguration(this ConfigurationManager configuration,
        IWebHostEnvironment environment)
    {
        // Settings for docker
        configuration.AddJsonFile("hostsettings.json", true);

        // Settings for ocelot
        configuration.SetBasePath(Directory.GetCurrentDirectory())
            .AddOcelot("Ocelot", environment)
            .AddEnvironmentVariables();

        // Settings for consul kv
        ConsulKVSettings consulKvSettings = new ();
        configuration.GetRequiredSection("ConsulKV").Bind(consulKvSettings);
        configuration.AddConsulKv(consulKvSettings);
    }
}