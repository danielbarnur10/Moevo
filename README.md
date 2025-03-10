

Task Management API Documentation

1. Overview

The Task Management API is a RESTful backend service built using ASP.NET Core. It is designed to manage projects and tasks with a focus on clean architecture and security. Key features include:
	•	User Authentication:
Integrated with AWS Cognito to manage user registration, login, and JWT-based authentication.
	•	Project Management:
CRUD (Create, Read, Update, Delete) operations for managing projects, where each project has a name and description.
	•	Database:
Utilizes Entity Framework Core with SQLite for data persistence.
	•	Architecture:
Follows an Onion/Clean Architecture pattern with clearly defined layers: Domain, Application, Infrastructure, and Presentation.
	•	Testing:
MSTest is used for unit testing, along with Moq for mocking dependencies and EF Core InMemory for testing data access.

⸻

2. Project Structure

TaskManagementAPI/
├── TaskManagementAPI.API/           // Presentation Layer (Web API)
│   ├── Controllers/
│   │   ├── AuthController.cs        // Handles user authentication endpoints
│   │   └── ProjectsController.cs    // Handles CRUD operations for projects
│   ├── Program.cs                   // Application startup and middleware configuration
│   └── appsettings.json             // Application configuration settings
├── TaskManagementAPI.Application/   // Application Layer
│   ├── DTOs/                        // Data Transfer Objects (e.g., ProjectDTO, RegisterDTO, LoginDTO)
│   ├── Interfaces/                  // Service interfaces (e.g., IProjectService, IAuthService)
│   └── Services/                    // Service implementations (e.g., ProjectService, AuthService)
├── TaskManagementAPI.Domain/        // Domain Layer
│   ├── Entities/                    // Domain entities (e.g., Project)
│   └── Interfaces/                  // Repository interfaces (e.g., IProjectRepository)
├── TaskManagementAPI.Infrastructure/// Infrastructure Layer
│   ├── Authentication/              // AWS Cognito authentication implementation (e.g., AuthService)
│   ├── Persistence/                 // EF Core DbContext and migrations (e.g., ApplicationDbContext)
│   ├── Repositories/                // Repository implementations (e.g., ProjectRepository)
│   └── DependencyInjection.cs       // Extension method for DI configuration
├── TaskManagementAPI.Tests/         // Test Project (using MSTest)
│   ├── AuthServiceTests.cs          // MSTest for authentication service
│   └── ProjectServiceTests.cs       // MSTest for project service using EF Core InMemory
└── Muevo.sln                       // Solution file



⸻

3. Setup & Configuration

3.1 Prerequisites
	•	.NET 9 SDK
	•	SQLite
	•	AWS account (for Cognito)
	•	AWS CLI (optional, for configuring AWS credentials)

3.2 Configuration Files

appsettings.json

This file contains your application settings, including database connection strings, logging, and AWS configuration:

{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=localdatabase.db"
  },
  "AWS": {
    "Region": "us-east-1",
    "UserPoolId": "us-east-1_tLdPUMvt7",
    "ClientId": "6s4jk8juj1l46ur9nnu1tifhsd"
  },
  "JwtBearer": {
    "Authority": "https://cognito-idp.us-east-1.amazonaws.com/us-east-1_tLdPUMvt7",
    "Audience": "6s4jk8juj1l46ur9nnu1tifhsd"
  }
}

	Note:
		•	Ensure the AWS region, UserPoolId, and ClientId match your AWS Cognito configuration.
	•	You can also set AWS credentials using environment variables or a shared credentials file.

3.3 Dependency Injection

The DependencyInjection.cs file in the Infrastructure layer registers services used throughout the application:

using Amazon.CognitoIdentityProvider;
using Microsoft.EntityFrameworkCore;
using TaskManagementAPI.Application.Interfaces;
using TaskManagementAPI.Application.Services;
using TaskManagementAPI.Domain.Interfaces;
using TaskManagementAPI.Infrastructure.Authentication;
using TaskManagementAPI.Infrastructure.Persistence;
using TaskManagementAPI.Infrastructure.Repositories;

namespace TaskManagementAPI.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlite(connectionString));
            services.AddScoped<IProjectRepository, ProjectRepository>();
            services.AddScoped<IProjectService, ProjectService>();
            services.AddScoped<IAuthService, AuthService>();
            return services;
        }
    }
}



⸻

4. Application Startup

Program.cs

This file configures the application, loads AWS options, and sets up middleware:

using TaskManagementAPI;
using TaskManagementAPI.Infrastructure;
using Amazon.Extensions.NETCore.Setup;
using Amazon.CognitoIdentityProvider;

var builder = WebApplication.CreateBuilder(args);

// Load configuration from appsettings.json
builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

// Load AWS options from configuration
var awsOptions = builder.Configuration.GetAWSOptions();
builder.Services.AddDefaultAWSOptions(awsOptions);
builder.Services.AddAWSService<IAmazonCognitoIdentityProvider>();

// Register custom infrastructure services
builder.Services.AddInfrastructure(builder.Configuration.GetConnectionString("DefaultConnection"));

// Register custom Cognito authentication (JWT)
builder.Services.AddCognitoAuthentication(builder.Configuration);

// Add controllers
builder.Services.AddControllers();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.Run();

ServiceExtensions.cs

Used to configure JWT Bearer authentication based on AWS Cognito:

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace TaskManagementAPI
{
    public static class ServiceExtensions
    {
        public static IServiceCollection AddCognitoAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            var cognitoSettings = configuration.GetSection("AWS");
            var userPoolId = cognitoSettings["UserPoolId"];
            var clientId = cognitoSettings["ClientId"];
            var region = cognitoSettings["Region"];

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.Authority = $"https://cognito-idp.{region}.amazonaws.com/{userPoolId}";
                    options.Audience = clientId;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = $"https://cognito-idp.{region}.amazonaws.com/{userPoolId}",
                        ValidateAudience = true,
                        ValidAudience = clientId,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ClockSkew = TimeSpan.Zero
                    };
                });

            return services;
        }
    }
}



⸻

5. AWS Cognito Authentication

5.1 AuthService

Handles user registration and login by interacting with AWS Cognito:

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
                return response.UserSub;
            }
            catch (Exception ex)
            {
                throw new Exception($"Registration failed: {ex.Message}");
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
                if (authResponse.AuthenticationResult == null)
                {
                    throw new Exception("Authentication failed: No authentication result returned.");
                }

                return authResponse.AuthenticationResult.AccessToken;
            }
            catch (Exception ex)
            {
                throw new Exception($"Login failed: {ex.Message}");
            }
        }
    }
}

5.2 AuthController

Exposes endpoints for user registration and login:

using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TaskManagementAPI.Application.DTOs;
using TaskManagementAPI.Application.Interfaces;

namespace TaskManagementAPI.API.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDTO registerDto)
        {
            try
            {
                var userId = await _authService.RegisterAsync(registerDto);
                return Ok(new { UserId = userId });
            }
            catch (System.Exception ex)
            {
                return NotFound(new { Message = ex.Message });
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO loginDto)
        {
            try
            {
                var token = await _authService.LoginAsync(loginDto);
                return Ok(new { Token = token });
            }
            catch (System.Exception ex)
            {
                return NotFound(new { Message = ex.Message });
            }
        }
    }
}



⸻

6. API Endpoints

6.1 Authentication Endpoints
	•	Register User:
	•	Method: POST
	•	URL: http://localhost:5172/api/auth/register
	•	Body (JSON):

{
  "Email": "user@example.com",
  "Password": "YourSecurePassword123"
}


	•	Response: Returns a JSON object with UserId if successful, or an error message with a 404 status code on failure.

	•	Login User:
	•	Method: POST
	•	URL: http://localhost:5172/api/auth/login
	•	Body (JSON):

{
  "Email": "user@example.com",
  "Password": "YourSecurePassword123"
}


	•	Response: Returns a JSON object with Token (the JWT access token) if successful, or an error message with a 404 status code on failure.

6.2 Project Management Endpoints
	•	Create Project (POST):
	•	URL: http://localhost:5172/api/projects
	•	Body (JSON):

{
  "Name": "New Project",
  "Description": "This is a test project."
}


	•	Get All Projects (GET):
	•	URL: http://localhost:5172/api/projects
	•	Get Project by ID (GET):
	•	URL: http://localhost:5172/api/projects/{id}
	•	Update Project (PUT):
	•	URL: http://localhost:5172/api/projects/{id}
	•	Body (JSON):

{
  "Name": "Updated Project",
  "Description": "Updated description."
}


	•	Delete Project (DELETE):
	•	URL: http://localhost:5172/api/projects/{id}

	Note:
If endpoints are secured with JWT, add an Authorization header with the Bearer token:
Authorization: Bearer <your_jwt_token>

⸻

7. Testing

7.1 Running Unit Tests

The MSTest project includes tests for:
	•	ProjectService: Uses an in-memory EF Core database to test CRUD operations.
	•	AuthService: Uses Moq to simulate AWS Cognito responses for registration and login.

To run tests from the command line:

dotnet test

7.2 Testing via Postman
	•	Obtain a JWT: Use the login endpoint to get a token.
	•	Access Protected Endpoints: Use the token in the Authorization header (Bearer token) for endpoints that require authentication.

⸻

8. Deployment Considerations
	•	Environment Variables:
Use environment variables or secret management (like AWS Secrets Manager) for sensitive information in production.
	•	Database Migrations:
Use EF Core migrations (dotnet ef migrations add <MigrationName> and dotnet ef database update) to manage your SQLite schema in production.
	•	Scaling:
Consider containerizing your application (using Docker) and deploying it to a cloud service (like AWS ECS, Kubernetes, etc.) if you expect high usage.

⸻

9. Troubleshooting

Common Issues:
	•	No RegionEndpoint Configured:
Ensure that the AWS configuration in appsettings.json or environment variables includes a valid region.
	•	Database Not Found:
Make sure to run EF Core migrations or use EnsureCreated() on the DbContext.
	•	Authentication Challenges:
Verify that your Cognito App Client has the proper auth flows enabled (e.g., USER_PASSWORD_AUTH) and that users are confirmed if required.

⸻

10. Additional Resources
	•	AWS Cognito Documentation
	•	Entity Framework Core Documentation
	•	ASP.NET Core Documentation

⸻


**For a project that needs to handle 10k users per day and includes a client-side application,** I’d recommend a deployment architecture that emphasizes scalability, security, and performance. Here’s an overview of a deployment strategy:

⸻

1. Containerize the API

Why:
Containerization (using Docker) ensures that your API runs consistently across different environments and makes scaling easier.

How:
	•	Dockerize your ASP.NET Core API.
	•	Use a multi-stage Dockerfile to build and publish your application.

⸻

2. Orchestrate with a Managed Service

Options:
	•	AWS ECS with Fargate:
Run containers without managing EC2 instances, with built-in auto-scaling and load balancing.
	•	AWS EKS (Kubernetes):
If you need advanced orchestration capabilities and are familiar with Kubernetes, EKS is a robust choice.

Why:
Managed container orchestration simplifies deployment, scaling, and management of containers, enabling you to handle high traffic and ensuring high availability.

⸻

3. Use a Scalable Database

Why:
SQLite is great for development but not suitable for production at scale.
How:
	•	Migrate to a managed relational database service such as Amazon RDS (PostgreSQL or SQL Server).
	•	Enable automatic backups, scaling, and high availability configurations (Multi-AZ deployment) to support 10k users daily.

⸻

4. Deploy the Client-Side Application Separately

Static Client Hosting Options:
	•	AWS S3 + CloudFront:
Host your static client-side application (e.g., a React, Angular, or Vue.js SPA) on S3, and serve it via CloudFront for low latency and global distribution.
	•	Dedicated Frontend Platforms:
Platforms like Netlify or Vercel offer excellent performance and CI/CD integration for client-side apps.

Benefits:
	•	Improved performance through CDN caching.
	•	Independent scaling of the frontend and backend.

⸻

5. Implement API Gateway (Optional)

Why:
	•	An API Gateway (e.g., AWS API Gateway) can handle routing, rate limiting, caching, and security (like request validation and authorization) for your API.
	•	It can offload SSL termination and integrate directly with AWS Cognito for authentication.

How:
	•	Configure API Gateway to forward requests to your backend service.
	•	Use it to enforce security policies and monitor usage.

⸻

6. Auto-Scaling and Load Balancing

For the API:
	•	Configure auto-scaling policies (via ECS/EKS) to automatically add or remove container instances based on CPU, memory, or request count.
	•	Use an Application Load Balancer (ALB) to distribute traffic evenly across your containers.

For the Database:
	•	Configure read replicas (if necessary) and scaling policies in RDS to handle increased load.

⸻

7. CI/CD Pipeline

Why:
Automating builds, tests, and deployments ensures that your application is always up-to-date and reduces manual errors.

How:
	•	Use tools like GitHub Actions, AWS CodePipeline, or Jenkins to automate your CI/CD process.
	•	Ensure that your container images are built, tested, and pushed to a container registry (e.g., Amazon ECR).

⸻

8. Monitoring, Logging, and Security

Monitoring and Logging:
	•	Use AWS CloudWatch for application logging and monitoring metrics.
	•	Set up alerts for abnormal behavior or performance issues.

Security:
	•	Ensure communication is secure (HTTPS) using SSL/TLS.
	•	Manage secrets and credentials securely using AWS Secrets Manager or IAM roles.
	•	Integrate AWS Cognito for user authentication and leverage API Gateway or your middleware to validate JWT tokens.

⸻

Summary
	1.	Containerize your API using Docker.
	2.	Deploy on a managed orchestration service like AWS ECS with Fargate or AWS EKS for scalability and high availability.
	3.	Use a managed database service (Amazon RDS) instead of SQLite.
	4.	Host the client-side application separately on AWS S3 with CloudFront (or on platforms like Netlify/Vercel).
	5.	Optionally use API Gateway to manage and secure API endpoints.
	6.	Configure auto-scaling and load balancing for both API and database layers.
	7.	Set up a CI/CD pipeline for automated deployments.
	8.	Implement robust monitoring, logging, and security practices.

This deployment strategy will help ensure that your project is resilient, scalable, and secure enough to handle 10k users per day while providing a smooth client-side experience.

