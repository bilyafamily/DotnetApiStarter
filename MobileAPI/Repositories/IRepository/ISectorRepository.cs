using MobileAPI.DTOs.Common;
using MobileAPI.DTOs.SectorDtos;
using MobileAPI.Models;

namespace MobileAPI.Repositories.IRepository;

public interface ISectorRepository
{
    Task<IEnumerable<Sector>> GetAllAsync();
    Task<Sector?> GetByIdAsync(Guid id);
    Task<Sector> CreateAsync(CreateSectorDto createSectorDto);
    Task<Sector?> UpdateAsync(Guid id, UpdateSectorDto updateSectorDto);
    Task<bool> DeleteAsync(Guid id);
    
    Task<bool> ExistsAsync(Guid id);
    
    Task<Sector?> GetByNameAsync(string name);
}