using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using AuthenticationService = TaskManagementAPI.Services.AuthenticationService;

public class AuthenticationServiceTests
{
    private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
    private readonly Mock<ILogger<AuthenticationService>> _loggerMock;
    private readonly AuthenticationService _authService;

    public AuthenticationServiceTests()
    {
        _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        _loggerMock = new Mock<ILogger<AuthenticationService>>();
        _authService = new AuthenticationService(
            _httpContextAccessorMock.Object,
            _loggerMock.Object
        );
    }

    [Fact]
    public void GetUserId_WhenUserIsAuthenticated_ReturnsUserId()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        httpContext.User = new System.Security.Claims.ClaimsPrincipal(
            new System.Security.Claims.ClaimsIdentity(
                new[] { new System.Security.Claims.Claim("sub", "test-user-id") }
            )
        );

        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

        // Act
        var userId = _authService.GetUserId();

        // Assert
        Assert.Equal("test-user-id", userId);
    }
}
