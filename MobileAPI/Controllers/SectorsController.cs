using System.Net;
using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MobileAPI.DTOs.Common;
using MobileAPI.DTOs.SectorDtos;
using MobileAPI.Repositories.IRepository;

namespace MobileAPI.Controllers;


    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Policy = "AnyAuthenticated")]
    public class SectorsController : ControllerBase
    {
        private readonly ISectorRepository _sectorRepository;
        private readonly IMapper _mapper;
        private readonly ResponseDto _response;

        public SectorsController(ISectorRepository sectorRepository, IMapper mapper)
        {
            _sectorRepository = sectorRepository;
            _mapper = mapper;
            _response = new ResponseDto();
        }

        [HttpGet]
        public async Task<ActionResult<ResponseDto>> GetAllSectors()
        {
            try
            {
                var sectors = await _sectorRepository.GetAllAsync();
                var sectorDtos = _mapper.Map<IEnumerable<SectorDto>>(sectors);

                _response.Result = sectorDtos;
                _response.Message = "Sectors retrieved successfully";
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.StatusCode = HttpStatusCode.InternalServerError;
                _response.Message = $"Error retrieving sectors: {ex.Message}";
                return StatusCode(500, _response);
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ResponseDto>> GetSectorById(Guid id)
        {
            try
            {
                var sector = await _sectorRepository.GetByIdAsync(id);
                if (sector == null)
                {
                    _response.StatusCode = HttpStatusCode.NotFound;
                    _response.Message = "Sector not found";
                    return NotFound(_response);
                }

                var sectorDto = _mapper.Map<SectorDto>(sector);
                _response.Result = sectorDto;
                _response.Message = "Sector retrieved successfully";
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.StatusCode = HttpStatusCode.InternalServerError;
                _response.Message = $"Error retrieving sector: {ex.Message}";
                return StatusCode(500, _response);
            }
        }

        [HttpPost]
        // [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ResponseDto>> CreateSector([FromBody] CreateSectorDto createSectorDto)
        {
            try
            {
                // Validate input
                if (string.IsNullOrWhiteSpace(createSectorDto.Name))
                {
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    _response.Message = "Sector name is required";
                    return BadRequest(_response);
                }

                // Check if sector with same name already exists
                var existingSector = await _sectorRepository.GetByNameAsync(createSectorDto.Name);
                if (existingSector != null)
                {
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    _response.Message = "Sector with this name already exists";
                    return BadRequest(_response);
                }

                // Create the sector
                var createdSector = await _sectorRepository.CreateAsync(createSectorDto);
                var sectorDto = _mapper.Map<SectorDto>(createdSector);

                _response.Result = sectorDto;
                _response.Message = "Sector created successfully";
                return StatusCode(201, _response);
            }
            catch (Exception ex)
            {
                _response.StatusCode = HttpStatusCode.InternalServerError;
                _response.Message = $"Error creating sector: {ex.Message}";
                return StatusCode(500, _response);
            }
        }

        [HttpPut("{id:guid}")]
        // [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ResponseDto>> UpdateSector(Guid id, [FromBody] UpdateSectorDto updateSectorDto)
        {
            try
            {
                // Validate input
                if (string.IsNullOrWhiteSpace(updateSectorDto.Name))
                {
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    _response.Message = "Sector name is required";
                    return BadRequest(_response);
                }

                // Check if sector exists
                var sectorExists = await _sectorRepository.ExistsAsync(id);
                if (!sectorExists)
                {
                    _response.StatusCode = HttpStatusCode.NotFound;
                    _response.Message = "Sector not found";
                    return NotFound(_response);
                }

                // Check if another sector with the same name exists
                var sectorWithSameName = await _sectorRepository.GetByNameAsync(updateSectorDto.Name);
                if (sectorWithSameName != null && sectorWithSameName.Id != id)
                {
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    _response.Message = "Another sector with this name already exists";
                    return BadRequest(_response);
                }

                // Update the sector
                var updatedSector = await _sectorRepository.UpdateAsync(id, updateSectorDto);
                if (updatedSector == null)
                {
                    _response.StatusCode = HttpStatusCode.NotFound;
                    _response.Message = "Sector not found during update";
                    return NotFound(_response);
                }

                var sectorDto = _mapper.Map<SectorDto>(updatedSector);
                _response.Result = sectorDto;
                _response.Message = "Sector updated successfully";
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.StatusCode = HttpStatusCode.InternalServerError;
                _response.Message = $"Error updating sector: {ex.Message}";
                return StatusCode(500, _response);
            }
        }

        [HttpDelete("{id}")]
        // [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ResponseDto>> DeleteSector(Guid id)
        {
            try
            {
                var sectorExists = await _sectorRepository.ExistsAsync(id);
                if (!sectorExists)
                {
                    _response.StatusCode = HttpStatusCode.NotFound;
                    _response.Message = "Sector not found";
                    return NotFound(_response);
                }

                var result = await _sectorRepository.DeleteAsync(id);
                if (!result)
                {
                    _response.StatusCode = HttpStatusCode.InternalServerError;
                    _response.Message = "Failed to delete sector";
                    return StatusCode(500, _response);
                }

                _response.Message = "Sector deleted successfully";
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.StatusCode = HttpStatusCode.InternalServerError;
                _response.Message = $"Error deleting sector: {ex.Message}";
                return StatusCode(500, _response);
            }
        }
    }
