namespace MobileAPI.DTOs.Auth;

public class VerifyResetTokenDto
{
    public string Token { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}