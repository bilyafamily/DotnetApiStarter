using AutoMapper;
using MobileAPI.Data;
using MobileAPI.DTOs.SectorDtos;
using MobileAPI.Models;
using MobileAPI.Repositories.IRepository;

namespace MobileAPI.Repositories;

using Microsoft.EntityFrameworkCore;

public class SectorRepository : ISectorRepository
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;


    public SectorRepository(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<IEnumerable<Sector>> GetAllAsync()
    {
        return await _context.Sectors
            .OrderBy(s => s.Name)
            .ToListAsync();
    }
    
    public async Task<Sector?> GetByIdAsync(Guid id)
    {
        return await _context.Sectors.FindAsync(id);
    }

    public async Task<Sector?> GetByNameAsync(string name)
    {
        return await _context.Sectors
            .FirstOrDefaultAsync(s => s.Name.ToLower() == name.ToLower());
    }

    public async Task<Sector> CreateAsync(CreateSectorDto sector)
    {
        var newItem = _mapper.Map<Sector>(sector);
        _context.Sectors.Add(newItem);
        await _context.SaveChangesAsync();
        return newItem;
    }

    public async Task<Sector?> UpdateAsync(Guid id, UpdateSectorDto updateSectorDto)
    {
        var existingSector = await _context.Sectors.FindAsync(id);
        if (existingSector == null)
            return null;
        
        _mapper.Map(updateSectorDto, existingSector);
        existingSector.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return existingSector;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var sector = await _context.Sectors.FindAsync(id);
        if (sector == null)
            return false;

        _context.Sectors.Remove(sector);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _context.Sectors.AnyAsync(s => s.Id == id);
    }
    

    public async Task<bool> ExistsByNameAsync(string name)
    {
        return await _context.Sectors.AnyAsync(s => s.Name.ToLower() == name.ToLower());
    }

}