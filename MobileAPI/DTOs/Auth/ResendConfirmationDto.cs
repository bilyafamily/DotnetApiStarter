using System.ComponentModel.DataAnnotations;

namespace MobileAPI.DTOs.Auth;

public class ResendConfirmationDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
}