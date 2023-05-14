using System.Text.Json;
using System.Text.Json.Serialization;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using ProjectManagement.ApiGateway.Configuration;
using Steeltoe.Management.Endpoint.Health;
using Steeltoe.Management.Endpoint.Info;
using Steeltoe.Management.Endpoint.Metrics;
using Steeltoe.Management.Endpoint.Refresh;
using Winton.Extensions.Configuration.Consul;

namespace ProjectManagement.ApiGateway.Extensions;

[ExcludeFromCodeCoverage]
public static class ConfigurationExtensions
{
    public static void AddApplicationConfiguration(
        this ConfigurationManager configuration,
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

    public static async Task<IApplicationBuilder> UseCustomOcelot(this IApplicationBuilder builder)
    {
        OcelotPipelineConfiguration config = GetOcelotConfiguration();
        await builder.UseOcelot(config);
        return builder;
    }

    private static JsonSerializerOptions GetSerializerOptions()
    {
        JsonSerializerOptions serializerOptions = new ();
        serializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;

        if (serializerOptions.DefaultIgnoreCondition != JsonIgnoreCondition.WhenWritingNull)
        {
            serializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        }

        if (serializerOptions.Converters?.Any(c => c is JsonStringEnumConverter) != true)
        {
            serializerOptions.Converters?.Add(new JsonStringEnumConverter());
        }

        if (serializerOptions.Converters?.Any(c => c is HealthConverter or HealthConverterV3) != true)
        {
            serializerOptions.Converters?.Add(new HealthConverter());
        }

        if (serializerOptions.Converters?.Any(c => c is MetricsResponseConverter) != true)
        {
            serializerOptions.Converters?.Add(new MetricsResponseConverter());
        }

        return serializerOptions;
    }

    private static OcelotPipelineConfiguration GetOcelotConfiguration()
    {
        return new OcelotPipelineConfiguration
        {
            PreErrorResponderMiddleware = async (ctx, next) =>
            {
                if (ctx.Request.Path.Equals(new PathString("/actuator/health")))
                {
                    IHealthEndpoint? healthEndpoint = ctx.RequestServices.GetService<IHealthEndpoint>();

                    if (healthEndpoint != null)
                    {
                        HealthEndpointResponse? response = healthEndpoint.Invoke(null);
                        await ctx.Response.WriteAsJsonAsync(response, GetSerializerOptions());
                        return;
                    }
                }
                else if (ctx.Request.Path.Equals(new PathString("/actuator/info")))
                {
                    InfoEndpoint? infoEndpoint = ctx.RequestServices.GetService<InfoEndpoint>();

                    if (infoEndpoint != null)
                    {
                        Dictionary<string, object>? response = infoEndpoint.Invoke();
                        await ctx.Response.WriteAsJsonAsync(response);
                        return;
                    }
                }
                else if (ctx.Request.Path.Equals(new PathString("/actuator/refresh")))
                {
                    IRefreshEndpoint? refreshEndpoint = ctx.RequestServices.GetService<IRefreshEndpoint>();

                    if (refreshEndpoint != null)
                    {
                        IList<string>? response = refreshEndpoint.Invoke();
                        await ctx.Response.WriteAsJsonAsync(response);
                        return;
                    }
                }

                await next.Invoke();
            },
        };
    }

    private static void AddConsulKv(this IConfigurationBuilder builder, ConsulKVSettings settings)
    {
        builder.AddConsul(settings.Key, options =>
        {
            options.ConsulConfigurationOptions = config =>
            {
                config.Address = new Uri(settings.Url);
                config.Token = settings.Token;
            };

            options.Optional = false;
            options.ReloadOnChange = true;
        });
    }
}