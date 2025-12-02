using MobileAPI.DTOs.Auth;
using MobileAPI.Models;

namespace MobileAPI.Services.IService;

public interface ITokenService
{
    Task<string> CreateTokenAsync(ApplicationUser user);

    Task<VerifyResetTokenResponseDto> VerifyResetTokenAsync(string email, string token);
}