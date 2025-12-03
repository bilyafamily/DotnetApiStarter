using System.ComponentModel.DataAnnotations;

namespace MobileAPI.DTOs.UserDtos;

public class UpdateUserDto
{
    [StringLength(50, MinimumLength = 2)]
    public string? FirstName { get; set; }
        
    [StringLength(50, MinimumLength = 2)]
    public string? LastName { get; set; }
        
    [Phone]
    public string? PhoneNumber { get; set; }
        
    public bool? IsActive { get; set; }
}