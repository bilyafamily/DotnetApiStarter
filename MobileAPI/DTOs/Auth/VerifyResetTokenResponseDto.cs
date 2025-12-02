namespace MobileAPI.DTOs.Auth;

public class VerifyResetTokenResponseDto
{
    public bool IsValid { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? Email { get; set; }
    public DateTime? ExpiryDate { get; set; }
}