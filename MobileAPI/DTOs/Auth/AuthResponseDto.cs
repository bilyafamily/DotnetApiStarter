namespace MobileAPI.DTOs.Auth;

public class AuthResponseDto
{
        public string Token { get; set; } = string.Empty;
        public string ExpiresIn { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
}