using TaskManagementAPI.Application.DTOs;
using TaskManagementAPI.Domain.Entities;

namespace TaskManagementAPI.Application.Interfaces
{
    public interface IProjectService
    {
        Task<IEnumerable<Project>> GetAllProjectsAsync();
        Task<Project?> GetProjectByIdAsync(int id);
        Task AddProjectAsync(ProjectDTO projectDto);
        Task UpdateProjectAsync(int id, ProjectDTO projectDto);
        Task DeleteProjectAsync(int id);
    }
}