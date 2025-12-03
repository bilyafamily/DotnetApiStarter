using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MobileAPI.DTOs.Common;
using MobileAPI.DTOs.UserDtos;
using MobileAPI.Services.IService;

namespace MobileAPI.Controllers;

    [Route("api/[controller]")]
    [ApiController]
    // [Authorize(Roles = "Admin")]
    public class UsersController : ControllerBase
    {
        private readonly IUserManagementService _userManagementService;
        private readonly ILogger<UsersController> _logger;
        private readonly ResponseDto _response;

        public UsersController(
            IUserManagementService userManagementService,
            ILogger<UsersController> logger)
        {
            _userManagementService = userManagementService;
            _logger = logger;
            _response = new ResponseDto();
        }

        // GET: api/users
        [HttpGet]
        public async Task<ResponseDto> GetAllUsers()
        {
            return await _userManagementService.GetAllUsersAsync();
        }

        // GET: api/users/{id}
        [HttpGet("{id}")]
        public async Task<ResponseDto> GetUserById(string id)
        {
            return await _userManagementService.GetUserByIdAsync(id);
        }

        // POST: api/users/create
        [HttpPost("create")]
        public async Task<ResponseDto> CreateUser([FromBody] CreateUserDto dto)
        {
            return await _userManagementService.CreateUserAsync(dto);
        }

        // PUT: api/users/{id}
        [HttpPut("{id}")]
        public async Task<ResponseDto> UpdateUser(string id, [FromBody] UpdateUserDto dto)
        {
            return await _userManagementService.UpdateUserAsync(id, dto);
        }

        // PATCH: api/users/{id}/status
        [HttpPatch("{id}/status")]
        public async Task<ResponseDto> ToggleUserStatus(string id, [FromBody] ToggleUserStatusDto dto)
        {
            if (id != dto.UserId)
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.Message = "User ID mismatch";
                return _response;
            }
            
            return await _userManagementService.ToggleUserStatusAsync(id, dto.IsActive);
        }

        // POST: api/users/reset-password
        [HttpPost("reset-password")]
        public async Task<ResponseDto> ResetPassword([FromBody] ResetUserPasswordDto dto)
        {
            return await _userManagementService.ResetPasswordAsync(dto);
        }

        // GET: api/users/{id}/roles
        [HttpGet("{id}/roles")]
        public async Task<ResponseDto> GetUserRoles(string id)
        {
            return await _userManagementService.GetUserRolesAsync(id);
        }
    }
