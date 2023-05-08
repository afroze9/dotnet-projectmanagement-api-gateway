using System.Diagnostics.CodeAnalysis;

namespace ProjectManagement.ApiGateway.Configuration;

/// <summary>
///     Represents settings related to telemetry and distributed tracing.
/// </summary>
[ExcludeFromCodeCoverage]
public class TelemetrySettings
{
    /// <summary>
    ///     Gets or sets the endpoint or URL where telemetry data will be sent.
    /// </summary>
    public string Endpoint { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the name of the service that is sending telemetry data.
    /// </summary>
    public string ServiceName { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the version of the service that is sending telemetry data.
    /// </summary>
    public string ServiceVersion { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets a value indicating whether the always-on sampler should be enabled or not.
    /// </summary>
    public bool EnableAlwaysOnSampler { get; set; } = false;

    /// <summary>
    ///     Gets or sets the probability of collecting a sample for a telemetry event.
    /// </summary>
    public double SampleProbability { get; set; } = 0.2;

    /// <summary>
    ///     Gets or sets a value indicating whether the console exporter should be enabled or not.
    /// </summary>
    public bool EnableConsoleExporter { get; set; } = false;
}