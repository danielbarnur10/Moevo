using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace TaskManagementAPI
{
    public static class ServiceExtensions
    {
        public static IServiceCollection AddCognitoAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            var cognitoSettings = configuration.GetSection("AWS:Cognito");
            var userPoolId = cognitoSettings["UserPoolId"];
            var appClientId = cognitoSettings["AppClientId"];
            var region = cognitoSettings["Region"];

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                options.Authority = $"https://cognito-idp.{region}.amazonaws.com/{userPoolId}";
                options.Audience = appClientId;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = $"https://cognito-idp.{region}.amazonaws.com/{userPoolId}",
                    ValidateAudience = true,
                    ValidAudience = appClientId,
                    ValidateLifetime = true
                };
            });

            return services;
        }
    }
}