namespace TaskManagementAPI.Domain.Entities
{
    public class TaskItem
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public TaskStatus Status { get; set; } = TaskStatus.Todo;
        public int ProjectId { get; set; }
        public Project? Project { get; set; }
    }

    public enum TaskStatus
    {
        Todo,
        InProgress,
        Done
    }
}