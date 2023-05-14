using System.Security.Claims;
using Ocelot.Authorization;
using Ocelot.Infrastructure.Claims.Parser;
using Ocelot.Responses;

namespace ProjectManagement.ApiGateway.Authorization;

public class DelimitedScopesAuthorizer : IScopesAuthorizer
{
    private readonly IClaimsParser _claimsParser;
    private readonly string _msScope = "http://schemas.microsoft.com/identity/claims/scope";
    private readonly string _scope = "scope";

    public DelimitedScopesAuthorizer(IClaimsParser claimsParser)
    {
        _claimsParser = claimsParser;
    }

    public Response<bool> Authorize(ClaimsPrincipal claimsPrincipal, List<string> routeAllowedScopes)
    {
        if (routeAllowedScopes == null || routeAllowedScopes.Count == 0)
        {
            return new OkResponse<bool>(true);
        }

        Response<List<string>>? values = _claimsParser.GetValuesByClaimType(claimsPrincipal.Claims, _msScope);

        if (!values.IsError && !values.Data.Any())
        {
            values = _claimsParser.GetValuesByClaimType(claimsPrincipal.Claims, _scope);
        }

        List<string> userScopes = new ();

        foreach (string? item in values.Data)
        {
            if (item.Contains(' '))
            {
                List<string> scopes = item.Split(' ').ToList();
                userScopes.AddRange(scopes);
            }
            else
            {
                userScopes.Add(item);
            }
        }

        List<string> matchesScopes = routeAllowedScopes.Intersect(userScopes).ToList();

        if (matchesScopes.Count == 0)
        {
            return new ErrorResponse<bool>(
                new ScopeNotAuthorizedError(
                    $"no one user scope: '{string.Join(",", userScopes)}' match with some allowed scope: '{string.Join(",", routeAllowedScopes)}'"));
        }

        return new OkResponse<bool>(true);
    }
}