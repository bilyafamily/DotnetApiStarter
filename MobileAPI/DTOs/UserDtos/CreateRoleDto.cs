using System.ComponentModel.DataAnnotations;

namespace MobileAPI.DTOs.UserDtos;

public class CreateRoleDto
{
    [Required]
    [StringLength(50, MinimumLength = 2)]
    public string RoleName { get; set; } = string.Empty;
}