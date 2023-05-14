using System.Security.Claims;
using Moq;
using Ocelot.Infrastructure.Claims.Parser;
using Ocelot.Responses;
using ProjectManagement.ApiGateway.Authorization;

namespace ProjectManagement.ApiGateway.UnitTests.Authorization;

[ExcludeFromCodeCoverage]
public class DelimitedScopesAuthorizerTests
{
    private readonly Mock<IClaimsParser> _mockClaimsParser = new ();

    [Fact]
    public void Authorize_NoScopes_ReturnsOk()
    {
        // arrange
        DelimitedScopesAuthorizer authorizer = new (_mockClaimsParser.Object);
        ClaimsPrincipal claims = new ();
        List<string> scopes = new ();

        // act
        Response<bool> result = authorizer.Authorize(claims, scopes);

        // assert
        Assert.IsType<OkResponse<bool>>(result);
        Assert.True(result.Data);
    }

    [Fact]
    public void Authorize_NoMatchingScopes_ReturnsError()
    {
        // arrange
        DelimitedScopesAuthorizer authorizer = new (_mockClaimsParser.Object);
        ClaimsPrincipal claims = new ();
        List<string> scopes = new () { "scope1", "scope2", "scope3" };
        _mockClaimsParser.Setup(x =>
                x.GetValuesByClaimType(It.IsAny<IEnumerable<Claim>>(),
                    "http://schemas.microsoft.com/identity/claims/scope"))
            .Returns(new OkResponse<List<string>>(new List<string>()));

        _mockClaimsParser.Setup(x => x.GetValuesByClaimType(It.IsAny<IEnumerable<Claim>>(), "scope"))
            .Returns(new OkResponse<List<string>>(new List<string>()));

        // act
        Response<bool> result = authorizer.Authorize(claims, scopes);

        // assert
        Assert.IsType<ErrorResponse<bool>>(result);
        Assert.Single(result.Errors);
        Assert.Equal("no one user scope: '' match with some allowed scope: 'scope1,scope2,scope3'",
            result.Errors[0].Message);
    }

    [Fact]
    public void Authorize_MatchingScopes_ReturnsOk()
    {
        // arrange
        DelimitedScopesAuthorizer authorizer = new (_mockClaimsParser.Object);
        ClaimsPrincipal claims = new ();
        List<string> scopes = new () { "scope1", "scope2", "scope3", "scope4 scope 5" };

        _mockClaimsParser.Setup(x =>
                x.GetValuesByClaimType(It.IsAny<IEnumerable<Claim>>(),
                    "http://schemas.microsoft.com/identity/claims/scope"))
            .Returns(new OkResponse<List<string>>(scopes));

        _mockClaimsParser.Setup(x => x.GetValuesByClaimType(It.IsAny<IEnumerable<Claim>>(), "scope"))
            .Returns(new OkResponse<List<string>>(scopes));

        // act
        Response<bool> result = authorizer.Authorize(claims, scopes);

        // assert
        Assert.IsType<OkResponse<bool>>(result);
        Assert.True(result.Data);
    }
}