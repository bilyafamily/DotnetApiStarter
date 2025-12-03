using System.ComponentModel.DataAnnotations;

namespace MobileAPI.DTOs.UserDtos;

public class ResetUserPasswordDto
{
    [Required]
    public string UserId { get; set; } = string.Empty;
        
    [Required]
    [StringLength(100, MinimumLength = 8)]
    public string NewPassword { get; set; } = string.Empty;
        
    [Required]
    [Compare("NewPassword")]
    public string ConfirmPassword { get; set; } = string.Empty;
}