using System;
using System.Collections.Generic;

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
        private readonly IAmazonCognitoIdentityProvider _cognitoClient;
        private readonly string _clientId;
        private readonly string _userPoolId;

        public AuthService(IAmazonCognitoIdentityProvider cognitoClient, IConfiguration config)
        {
            _cognitoClient = cognitoClient;
            _clientId = config["AWS:ClientId"];
            _userPoolId = config["AWS:UserPoolId"];

        }

        public async Task<string> RegisterAsync(RegisterDTO registerDto)
        {
            try
            {
                var signUpRequest = new SignUpRequest
                {
                    ClientId = _clientId,
                    Username = registerDto.Email,
                    Password = registerDto.Password,
                    UserAttributes = new List<AttributeType>
                    {
                        new AttributeType { Name = "email", Value = registerDto.Email },
                    },
                };

                var response = await _cognitoClient.SignUpAsync(signUpRequest);

                return response.UserSub; // Returns Cognito User ID
            }
            catch (Exception ex)
            {
                throw new Exception($"Login failed: {ex.Message}");
            }
        }

        public async Task<string> LoginAsync(LoginDTO loginDto)
        {
            try
            {
                var authRequest = new InitiateAuthRequest
                {
                    AuthFlow = AuthFlowType.USER_PASSWORD_AUTH,
                    ClientId = _clientId,
                    AuthParameters = new Dictionary<string, string>
                    {
                        { "USERNAME", loginDto.Email },
                        { "PASSWORD", loginDto.Password },
                    },
                };

                var authResponse = await _cognitoClient.InitiateAuthAsync(authRequest);
                if (authResponse?.AuthenticationResult == null)
                {
                    // Throw an exception or handle it as a failed login
                    throw new Exception("Authentication failed: No authentication result returned.");
                }
                var accessToken = authResponse.AuthenticationResult.AccessToken;

                return accessToken; // Return JWT Token
            }
            catch (Exception ex)
            {
                throw new Exception($"Login failed: {ex.Message}");
            }
        }
    }
}
