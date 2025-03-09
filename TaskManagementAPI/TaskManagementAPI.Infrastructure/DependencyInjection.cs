using Microsoft.Extensions.DependencyInjection;
using TaskManagementAPI.Services;
using TaskManagementAPI.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace TaskManagementAPI.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Database Context (Using PostgreSQL or InMemory for Testing)
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"))); // Change to UseSqlServer if using MSSQL

            // Authentication - AWS Cognito (JWT Bearer Token)
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.Authority = configuration["AWS:Cognito:Authority"];
                    options.Audience = configuration["AWS:Cognito:ClientId"];
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true
                    };
                });

            // Authorization
            services.AddAuthorization();

            // Register Services
            services.AddScoped<IProjectService, ProjectService>();
            services.AddScoped<ITaskService, TaskService>();
            services.AddScoped<IAuthenticationService, AuthenticationService>();

            // Logging
            services.AddLogging();

            return services;
        }
    }
}