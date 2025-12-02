using MobileAPI.DTOs.Common;

namespace MobileAPI.Services.IService;

public interface IRoleService
{
    Task<ResponseDto> CreateRoleAsync(string roleName);
    Task<ResponseDto> DeleteRoleAsync(string roleName);
    Task<ResponseDto> AssignRoleToUserAsync(string email, string roleName);
    Task<ResponseDto> RemoveRoleFromUserAsync(string email, string roleName);
    Task<ResponseDto> GetUserRolesAsync(string email);
    Task<ResponseDto> GetAllRolesAsync();
}