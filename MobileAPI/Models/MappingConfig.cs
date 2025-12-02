using AutoMapper;
using MobileAPI.DTOs.SectorDtos;

namespace MobileAPI.Models;

public class MappingConfig : Profile
{
    public MappingConfig()
    {
        CreateMap<Sector, CreateSectorDto>().ReverseMap();
        CreateMap<Sector, SectorDto>().ReverseMap();
        // Add other mappings here
    }
}