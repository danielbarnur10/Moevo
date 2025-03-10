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
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services,
            string connectionString
        )
        {
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlite(connectionString)
            );
            services.AddScoped<IProjectRepository, ProjectRepository>();
            services.AddScoped<IProjectService, ProjectService>();
            services.AddScoped<IAuthService, AuthService>();
            return services;
        }
    }
}
