namespace TaskManagementAPI.Application.DTOs
{
    public class RegisterDTO
    {
        public required string Email { get; set; }
        public required string Password { get; set; }
    }
}
