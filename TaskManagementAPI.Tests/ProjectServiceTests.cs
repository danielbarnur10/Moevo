using Microsoft.EntityFrameworkCore;
using TaskManagementAPI.Application.DTOs;
using TaskManagementAPI.Application.Interfaces;
using TaskManagementAPI.Application.Services;
using TaskManagementAPI.Domain.Entities;
using TaskManagementAPI.Domain.Interfaces;
using TaskManagementAPI.Infrastructure.Persistence;

namespace TaskManagementAPI.Tests
{
    [TestClass]
    public class ProjectServiceTests
    {
        private IProjectService _projectService;
        private ApplicationDbContext _context;

        [TestInitialize]
        public void TestInitialize()
        {
            // Create an in-memory database for testing
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDb")
                .Options;
            _context = new ApplicationDbContext(options);

            // Seed some test data
            _context.Projects.AddRange(
                new Project
                {
                    Id = 6,
                    Name = "Project 1",
                    Description = "Desc 1",
                },
                new Project
                {
                    Id = 7,
                    Name = "Project 2",
                    Description = "Desc 2",
                }
            );
            _context.SaveChanges();

            // Create a simple repository for testing
            IProjectRepository projectRepository = new RepositoryForTesting(_context);

            // Instantiate the service (assuming manual mapping in the service)
            _projectService = new ProjectService(projectRepository);
        }

        [TestMethod]
        public async Task GetAllProjectsAsync_ReturnsAllProjects()
        {
            // Act
            IEnumerable<Project> projects = await _projectService.GetAllProjectsAsync();

            // Assert
            Assert.IsNotNull(projects);
            Assert.AreEqual(2, projects.Count());
        }

        [TestMethod]
        public async Task AddProjectAsync_CreatesNewProject()
        {
            // Arrange
            var projectDto = new ProjectDTO { Name = "Project 3", Description = "Desc 3" };

            // Act
            await _projectService.AddProjectAsync(projectDto);
            var projects = await _projectService.GetAllProjectsAsync();

            // Assert
            Assert.AreEqual(3, projects.Count());
            Assert.IsTrue(projects.Any(p => p.Name == "Project 3"));
        }

        [TestCleanup]
        public void TestCleanup()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }

    // A simple implementation of IProjectRepository for testing purposes
    public class RepositoryForTesting : IProjectRepository
    {
        private readonly ApplicationDbContext _context;

        public RepositoryForTesting(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddProjectAsync(Project project)
        {
            await _context.Projects.AddAsync(project);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteProjectAsync(int id)
        {
            var project = await _context.Projects.FindAsync(id);
            if (project != null)
            {
                _context.Projects.Remove(project);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<Project>> GetAllProjectsAsync()
        {
            return await _context.Projects.ToListAsync();
        }

        public async Task<Project?> GetProjectByIdAsync(int id)
        {
            return await _context.Projects.FindAsync(id);
        }

        public async Task UpdateProjectAsync(Project project)
        {
            _context.Projects.Update(project);
            await _context.SaveChangesAsync();
        }
    }
}
