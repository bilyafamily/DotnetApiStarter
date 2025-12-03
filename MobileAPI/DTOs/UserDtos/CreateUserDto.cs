using System.ComponentModel.DataAnnotations;

namespace MobileAPI.DTOs.UserDtos;

public class CreateUserDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
        
    [Required]
    [StringLength(50, MinimumLength = 2)]
    public string FirstName { get; set; } = string.Empty;
        
    [Required]
    [StringLength(50, MinimumLength = 2)]
    public string LastName { get; set; } = string.Empty;
        
    [Phone]
    public string? PhoneNumber { get; set; }
        
    [Required]
    [StringLength(100, MinimumLength = 8)]
    public string Password { get; set; } = string.Empty;
        
    [Required]
    [Compare("Password")]
    public string ConfirmPassword { get; set; } = string.Empty;
        
    public List<string> Roles { get; set; } = new List<string>();
}