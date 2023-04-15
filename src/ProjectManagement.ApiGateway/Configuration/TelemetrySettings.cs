using System.Diagnostics.CodeAnalysis;

namespace ProjectManagement.ApiGateway.Configuration;

[ExcludeFromCodeCoverage]
public class TelemetrySettings
{
    public string Endpoint { get; set; } = string.Empty;

    public string ServiceName { get; set; } = string.Empty;

    public string ServiceVersion { get; set; } = string.Empty;
}