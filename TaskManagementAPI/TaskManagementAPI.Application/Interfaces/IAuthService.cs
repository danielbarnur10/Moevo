using System.Threading.Tasks;
using TaskManagementAPI.Application.DTOs;

namespace TaskManagementAPI.Application.Interfaces
{
    public interface IAuthService
    {
        Task<string> RegisterAsync(RegisterDTO registerDto);
        Task<string> LoginAsync(LoginDTO loginDto);
    }
}
