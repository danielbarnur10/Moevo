using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using Amazon.CognitoIdentityProvider;
using Amazon.CognitoIdentityProvider.Model;
using TaskManagementAPI.Application.DTOs;
using TaskManagementAPI.Application.Interfaces;
using TaskManagementAPI.Infrastructure.Authentication;
using Microsoft.Extensions.Configuration;

namespace TaskManagementAPI.Tests
{
    [TestClass]
    public class AuthServiceTests
    {
        private Mock<IAmazonCognitoIdentityProvider> _mockCognitoProvider;
        private IAuthService _authService;
        private readonly string _clientId = "dummyClientId";
        private readonly string _userPoolId = "dummyUserPoolId";

        [TestInitialize]
        public void Setup()
        {
            _mockCognitoProvider = new Mock<IAmazonCognitoIdentityProvider>();

            // Setup a dummy response for the SignUpAsync method
            _mockCognitoProvider.Setup(x => x.SignUpAsync(It.IsAny<SignUpRequest>(), default))
                .ReturnsAsync(new SignUpResponse { UserSub = "dummyUserSub" });

            // Setup a dummy response for the InitiateAuthAsync method
            _mockCognitoProvider.Setup(x => x.InitiateAuthAsync(It.IsAny<InitiateAuthRequest>(), default))
                .ReturnsAsync(new InitiateAuthResponse
                {
                    AuthenticationResult = new AuthenticationResultType
                    {
                        AccessToken = "dummyAccessToken"
                    }
                });

            // Create in-memory configuration for AWS settings
            var inMemorySettings = new Dictionary<string, string>
            {
                {"AWS:ClientId", _clientId},
                {"AWS:UserPoolId", _userPoolId}
            };
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            _authService = new AuthService(_mockCognitoProvider.Object, configuration);
        }

        [TestMethod]
        public async Task RegisterAsync_ReturnsUserSub()
        {
            // Arrange
            var registerDto = new RegisterDTO
            {
                Email = "test@example.com",
                Password = "TestPassword123!"
            };

            // Act
            var result = await _authService.RegisterAsync(registerDto);

            // Assert
            Assert.AreEqual("dummyUserSub", result);
        }

        [TestMethod]
        public async Task LoginAsync_ReturnsAccessToken()
        {
            // Arrange
            var loginDto = new LoginDTO
            {
                Email = "test@example.com",
                Password = "TestPassword123!"
            };

            // Act
            var token = await _authService.LoginAsync(loginDto);

            // Assert
            Assert.AreEqual("dummyAccessToken", token);
        }
    }
}