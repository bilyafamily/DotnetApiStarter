using System.ComponentModel.DataAnnotations;

namespace MobileAPI.DTOs.UserDtos;

public class AssignUserRoleDto
{
    [Required]
    public string UserId { get; set; } = string.Empty;
        
    [Required]
    public string RoleName { get; set; } = string.Empty;
}