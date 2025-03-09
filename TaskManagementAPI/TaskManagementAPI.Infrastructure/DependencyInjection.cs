using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TaskManagementAPI.Application.Interfaces;
using TaskManagementAPI.Application.Services;
using TaskManagementAPI.Domain.Interfaces;
using TaskManagementAPI.Infrastructure.Persistence;

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

            return services;
        }
    }
}