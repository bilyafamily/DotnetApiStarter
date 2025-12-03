using MobileAPI.DTOs.Auth;
using MobileAPI.DTOs.Common;
using MobileAPI.DTOs.UserDtos;

namespace MobileAPI.Services.IService;

public interface IUserManagementService
{
    Task<ResponseDto> GetAllUsersAsync();
    Task<ResponseDto> GetUserByIdAsync(string userId);
    Task<ResponseDto> CreateUserAsync(CreateUserDto dto);
    Task<ResponseDto> UpdateUserAsync(string userId, UpdateUserDto dto);
    Task<ResponseDto> ToggleUserStatusAsync(string userId, bool isActive);
    Task<ResponseDto> ResetPasswordAsync(ResetUserPasswordDto dto);
    Task<ResponseDto> GetUserRolesAsync(string userId);
    Task<ResponseDto> GetAllRolesWithCountAsync();
}