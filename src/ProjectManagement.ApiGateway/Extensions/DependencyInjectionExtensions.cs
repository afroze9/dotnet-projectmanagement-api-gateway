﻿using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Ocelot.Authorization;
using Ocelot.DependencyInjection;
using Ocelot.Provider.Consul;
using ProjectManagement.ApiGateway.Configuration;
using Winton.Extensions.Configuration.Consul;

namespace ProjectManagement.ApiGateway.Extensions;

public static class DependencyInjectionExtensions
{
    public static void AddConsulKv(this IConfigurationBuilder builder, ConsulKVSettings settings)
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

    // Github Issue: https://github.com/ThreeMammals/Ocelot/issues/913
    private static void AddGateway(this IServiceCollection services)
    {
        services
            .AddOcelot()
            .AddConsul();

        services.RemoveAll<IScopesAuthorizer>();
        services.TryAddSingleton<IScopesAuthorizer, DelimitedScopesAuthorizer>();
    }

    public static void RegisterDependencies(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSecurity(configuration);
        services.AddGateway();

        services.AddCors(options =>
        {
            options.AddPolicy("AllowAll", policy => { policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod(); });
        });
    }
}