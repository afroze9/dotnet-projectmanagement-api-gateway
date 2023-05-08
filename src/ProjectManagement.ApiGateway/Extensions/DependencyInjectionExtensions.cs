using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Ocelot.Authorization;
using Ocelot.DependencyInjection;
using Ocelot.Provider.Consul;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using ProjectManagement.ApiGateway.Configuration;
using Steeltoe.Discovery.Client;
using Steeltoe.Management.Endpoint;
using Steeltoe.Management.Endpoint.Health;
using Steeltoe.Management.Endpoint.Info;
using Steeltoe.Management.Endpoint.Refresh;

namespace ProjectManagement.ApiGateway.Extensions;

public static class DependencyInjectionExtensions
{
    private static void AddActuators(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHealthActuator(configuration);
        services.AddInfoActuator(configuration);
        services.AddHealthChecks();
        services.AddRefreshActuator();
        services.ActivateActuatorEndpoints();
    }

    // Github Issue: https://github.com/ThreeMammals/Ocelot/issues/913
    private static void AddGateway(this IServiceCollection services)
    {
        services
            .AddOcelot()
            .AddConsul();

        services.RemoveAll<IScopesAuthorizer>();
        services.TryAddSingleton<IScopesAuthorizer, DelimitedScopesAuthorizer>();
    }
    
    private static void AddSecurity(this IServiceCollection services, IConfiguration configuration)
    {
        Auth0Settings auth0Settings = new ();
        configuration.GetRequiredSection(nameof(Auth0Settings)).Bind(auth0Settings);

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer("Auth0", options =>
        {
            options.Authority = auth0Settings.Authority;
            options.Audience = auth0Settings.Audience;
        });
    }
    
    private static void AddTelemetry(this IServiceCollection services, IConfiguration configuration)
    {
        TelemetrySettings telemetrySettings = new ();
        configuration.GetRequiredSection(nameof(TelemetrySettings)).Bind(telemetrySettings);

        services
            .AddOpenTelemetry()
            .WithTracing(builder =>
                {
                    builder
                        .AddHttpClientInstrumentation()
                        .AddAspNetCoreInstrumentation()
                        .ConfigureResource(options =>
                        {
                            options.AddService(
                                telemetrySettings.ServiceName,
                                serviceVersion: telemetrySettings.ServiceVersion,
                                autoGenerateServiceInstanceId: true);
                        })
                        .AddOtlpExporter(options => { options.Endpoint = new Uri(telemetrySettings.Endpoint); });

                    if (telemetrySettings.EnableConsoleExporter)
                    {
                        builder.AddConsoleExporter();
                    }

                    if (telemetrySettings.EnableAlwaysOnSampler)
                    {
                        builder.SetSampler<AlwaysOnSampler>();
                    }
                    else
                    {
                        builder.SetSampler(new TraceIdRatioBasedSampler(telemetrySettings.SampleProbability));
                    }
                }
            )
            .WithMetrics(builder =>
            {
                builder
                    .AddAspNetCoreInstrumentation()
                    .AddConsoleExporter()
                    .AddOtlpExporter(options => { options.Endpoint = new Uri(telemetrySettings.Endpoint); });
            });
    }

    public static void RegisterDependencies(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddActuators(configuration);
        services.AddDiscoveryClient(configuration);
        services.AddSecurity(configuration);
        services.AddGateway();
        services.AddTelemetry(configuration);

        services.AddCors(options =>
        {
            options.AddPolicy("AllowAll", policy => { policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod(); });
        });
    }
}