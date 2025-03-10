using System;
using System.Threading.Tasks;
using Amazon.CognitoIdentityProvider;
using Amazon.CognitoIdentityProvider.Model;
using Microsoft.Extensions.Configuration;
using TaskManagementAPI.Application.DTOs;
using TaskManagementAPI.Application.Interfaces;

namespace TaskManagementAPI.Infrastructure.Authentication
{
    public class AuthService : IAuthService
    {
        private readonly IAmazonCognitoIdentityProvider _cognitoProvider;
        private readonly string _userPoolId;
        private readonly string _appClientId;

        public AuthService(
            IAmazonCognitoIdentityProvider cognitoProvider,
            IConfiguration configuration
        )
        {
            _cognitoProvider = cognitoProvider;
            var cognitoSettings = configuration.GetSection("AWS");
            _userPoolId = cognitoSettings["UserPoolId"];
            _appClientId = cognitoSettings["ClientId"];
        }

        public async Task<string> RegisterAsync(RegisterDTO registerDto)
        {
            var request = new SignUpRequest
            {
                ClientId = _appClientId,
                Username = registerDto.Email,
                Password = registerDto.Password,
                UserAttributes = new List<AttributeType>
                {
                    new AttributeType { Name = "email", Value = registerDto.Email },
                },
            };

            var response = await _cognitoProvider.SignUpAsync(request);
            return response.UserSub; // Cognito User ID
        }

        public async Task<string> LoginAsync(LoginDTO loginDto)
        {
            var request = new InitiateAuthRequest
            {
                ClientId = _appClientId,
                AuthFlow = AuthFlowType.USER_PASSWORD_AUTH,
                AuthParameters = new Dictionary<string, string>
                {
                    { "USERNAME", loginDto.Email },
                    { "PASSWORD", loginDto.Password },
                },
            };

            var response = await _cognitoProvider.InitiateAuthAsync(request);
            return response.AuthenticationResult?.IdToken ?? string.Empty;
        }

        public async Task<bool> ValidateTokenAsync(string token)
        {
            // Token validation logic can go here
            return true;
        }
    }
}
