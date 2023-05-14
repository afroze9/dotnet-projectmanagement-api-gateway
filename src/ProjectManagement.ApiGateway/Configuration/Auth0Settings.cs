namespace ProjectManagement.ApiGateway.Configuration;

[ExcludeFromCodeCoverage]
public class Auth0Settings
{
    public string Authority { get; set; } = string.Empty;

    public string Audience { get; set; } = string.Empty;
}