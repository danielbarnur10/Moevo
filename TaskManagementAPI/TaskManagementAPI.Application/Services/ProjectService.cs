using AutoMapper;
using TaskManagementAPI.Application.DTOs;
using TaskManagementAPI.Application.Interfaces;
using TaskManagementAPI.Domain.Entities;
using TaskManagementAPI.Domain.Interfaces;

namespace TaskManagementAPI.Application.Services
{
    public class ProjectService : IProjectService
    {
        private readonly IProjectRepository _projectRepository;
        private readonly IMapper _mapper;

        public ProjectService(IProjectRepository projectRepository, IMapper mapper)
        {
            _projectRepository = projectRepository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<Project>> GetAllProjectsAsync()
        {
            return await _projectRepository.GetAllProjectsAsync();
        }

        public async Task<Project?> GetProjectByIdAsync(int id)
        {
            return await _projectRepository.GetProjectByIdAsync(id);
        }

        public async Task<Project> AddProjectAsync(ProjectDTO projectDto)
        {
            var project = _mapper.Map<Project>(projectDto);
            await _projectRepository.AddProjectAsync(project);
            return project;
        }

        public async Task UpdateProjectAsync(int id, ProjectDTO projectDto)
        {
            var project = await _projectRepository.GetProjectByIdAsync(id);
            if (project == null)
            {
                throw new Exception("Project not found");
            }
            _mapper.Map(projectDto, project);
            await _projectRepository.UpdateProjectAsync(project);
        }

        public async Task DeleteProjectAsync(int id)
        {
            await _projectRepository.DeleteProjectAsync(id);
        }
    }
}