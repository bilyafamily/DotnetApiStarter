using System.ComponentModel.DataAnnotations;

namespace MobileAPI.Models;

public class Sector
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}