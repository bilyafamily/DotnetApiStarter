// Controllers/RolesController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MobileAPI.DTOs.Auth;
using MobileAPI.DTOs.Common;
using MobileAPI.DTOs.UserDtos;
using MobileAPI.Services.IService;
using CreateRoleDto = MobileAPI.DTOs.UserDtos.CreateRoleDto;

namespace MobileAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    // [Authorize(Roles = "Servicom.Admin")]
    // [Authorize(Roles = "Servicom.Admin")]
    public class RolesController : ControllerBase
    {
        private readonly IRoleService _roleService;
        private readonly IUserManagementService _userManagementService;
        private readonly ILogger<RolesController> _logger;
        private readonly ResponseDto _response;

        public RolesController(
            IRoleService roleService,
            IUserManagementService userManagementService,
            ILogger<RolesController> logger)
        {
            _roleService = roleService;
            _userManagementService = userManagementService;
            _logger = logger;
            _response = new ResponseDto();
        }

        // GET: api/roles/all
        [HttpGet("all")]
        public async Task<ResponseDto> GetAllRoles()
        {
            return await _userManagementService.GetAllRolesWithCountAsync();
        }

        // POST: api/roles/create
        [HttpPost("create")]
        public async Task<ResponseDto> CreateRole([FromBody] CreateRoleDto dto)
        {
            return await _roleService.CreateRoleAsync(dto.RoleName);
        }

        // DELETE: api/roles/{roleName}
        [HttpDelete("{roleName}")]
        public async Task<ResponseDto> DeleteRole(string roleName)
        {
            return await _roleService.DeleteRoleAsync(roleName);
        }

        // POST: api/roles/assign
        [HttpPost("assign")]
        public async Task<ResponseDto> AssignRoleToUser([FromBody] AssignUserRoleDto dto)
        {
            return await _roleService.AssignRoleToUserAsync(dto.UserId, dto.RoleName);
        }

        // POST: api/roles/remove
        [HttpPost("remove")]
        public async Task<ResponseDto> RemoveRoleFromUser([FromBody] AssignUserRoleDto dto)
        {
            return await _roleService.RemoveRoleFromUserAsync(dto.UserId, dto.RoleName);
        }

        // GET: api/roles/user/{userId}
        [HttpGet("user/{userId}")]
        public async Task<ResponseDto> GetUserRoles(string userId)
        {
            return await _roleService.GetUserRolesAsync(userId);
        }
    }
}