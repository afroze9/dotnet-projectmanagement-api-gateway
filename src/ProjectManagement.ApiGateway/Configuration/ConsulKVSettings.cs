using System.Diagnostics.CodeAnalysis;

namespace ProjectManagement.ApiGateway.Configuration;

[ExcludeFromCodeCoverage]
public class ConsulKVSettings
{
    public string Url { get; set; } = string.Empty;

    public string Token { get; set; } = string.Empty;

    public string Key { get; set; } = string.Empty;
}