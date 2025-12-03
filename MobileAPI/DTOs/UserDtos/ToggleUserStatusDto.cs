using System.ComponentModel.DataAnnotations;

namespace MobileAPI.DTOs.UserDtos;

public class ToggleUserStatusDto
{
    [Required]
    public string UserId { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}