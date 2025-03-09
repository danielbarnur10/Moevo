using TaskManagementAPI.Application.DTOs;
using TaskManagementAPI.Application.Interfaces;
using TaskManagementAPI.Domain.Entities;
using TaskManagementAPI.Domain.Interfaces;

namespace TaskManagementAPI.Application.Services
{
    public class ProjectService : IProjectService
    {
        private readonly IProjectRepository _projectRepository;

        public ProjectService(IProjectRepository projectRepository)
        {
            _projectRepository = projectRepository;
        }

        public async Task<IEnumerable<Project>> GetAllProjectsAsync()
        {
            return await _projectRepository.GetAllProjectsAsync();
        }

        public async Task<Project?> GetProjectByIdAsync(int id)
        {
            return await _projectRepository.GetProjectByIdAsync(id);
        }

        public async Task AddProjectAsync(ProjectDTO projectDto)
        {
            var project = new Project
            {
                Name = projectDto.Name,
                Description = projectDto.Description
            };
            await _projectRepository.AddProjectAsync(project);
        }

        public async Task UpdateProjectAsync(int id, ProjectDTO projectDto)
        {
            var project = await _projectRepository.GetProjectByIdAsync(id);
            if (project == null) return;

            project.Name = projectDto.Name;
            project.Description = projectDto.Description;

            await _projectRepository.UpdateProjectAsync(project);
        }

        public async Task DeleteProjectAsync(int id)
        {
            await _projectRepository.DeleteProjectAsync(id);
        }
    }
}