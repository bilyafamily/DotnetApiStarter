using System.Net;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MobileAPI.DTOs.Common;
using MobileAPI.Models;
using MobileAPI.Services.IService;

namespace MobileAPI.Services;

public class RoleService : IRoleService
{
     private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly ResponseDto _response;

    public RoleService(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _response = new ResponseDto();
    }

    public async Task<ResponseDto> CreateRoleAsync(string roleName)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(roleName))
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.Message = "Role name cannot be empty";
                return _response;
            }

            var roleExists = await _roleManager.RoleExistsAsync(roleName);
            if (roleExists)
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.Message = $"Role '{roleName}' already exists";
                return _response;
            }

            var result = await _roleManager.CreateAsync(new IdentityRole(roleName));
            if (result.Succeeded)
            {
                _response.Message = $"Role '{roleName}' created successfully";
                return _response;
            }

            _response.StatusCode = HttpStatusCode.BadRequest;
            _response.Message = string.Join(", ", result.Errors.Select(e => e.Description));
            return _response;
        }
        catch (Exception e)
        {
            _response.Message = e.Message;
            _response.StatusCode = HttpStatusCode.InternalServerError;
            return _response;
        }
    }

    public async Task<ResponseDto> DeleteRoleAsync(string roleName)
    {
        try
        {
            var role = await _roleManager.FindByNameAsync(roleName);
            if (role == null)
            {
                _response.StatusCode = HttpStatusCode.NotFound;
                _response.Message = $"Role '{roleName}' not found";
                return _response;
            }

            var result = await _roleManager.DeleteAsync(role);
            if (result.Succeeded)
            {
                _response.Message = $"Role '{roleName}' deleted successfully";
                return _response;
            }

            _response.StatusCode = HttpStatusCode.BadRequest;
            _response.Message = string.Join(", ", result.Errors.Select(e => e.Description));
            return _response;
        }
        catch (Exception e)
        {
            _response.Message = e.Message;
            _response.StatusCode = HttpStatusCode.InternalServerError;
            return _response;
        }
    }

    public async Task<ResponseDto> AssignRoleToUserAsync(string userId, string roleName)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                _response.StatusCode = HttpStatusCode.NotFound;
                _response.Message = "User not found";
                return _response;
            }

            var roleExists = await _roleManager.RoleExistsAsync(roleName);
            if (!roleExists)
            {
                _response.StatusCode = HttpStatusCode.NotFound;
                _response.Message = $"Role '{roleName}' not found";
                return _response;
            }

            var userAlreadyInRole = await _userManager.IsInRoleAsync(user, roleName);
            if (userAlreadyInRole)
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.Message = $"User is already in role '{roleName}'";
                return _response;
            }

            var result = await _userManager.AddToRoleAsync(user, roleName);
            if (result.Succeeded)
            {
                _response.Message = $"Role '{roleName}' assigned to user successfully";
                return _response;
            }

            _response.StatusCode = HttpStatusCode.BadRequest;
            _response.Message = string.Join(", ", result.Errors.Select(e => e.Description));
            return _response;
        }
        catch (Exception e)
        {
            _response.Message = e.Message;
            _response.StatusCode = HttpStatusCode.InternalServerError;
            return _response;
        }
    }

    public async Task<ResponseDto> RemoveRoleFromUserAsync(string userId, string roleName)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                _response.StatusCode = HttpStatusCode.NotFound;
                _response.Message = "User not found";
                return _response;
            }

            var userInRole = await _userManager.IsInRoleAsync(user, roleName);
            if (!userInRole)
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.Message = $"User is not in role '{roleName}'";
                return _response;
            }

            var result = await _userManager.RemoveFromRoleAsync(user, roleName);
            if (result.Succeeded)
            {
                _response.Message = $"Role '{roleName}' removed from user successfully";
                return _response;
            }

            _response.StatusCode = HttpStatusCode.BadRequest;
            _response.Message = string.Join(", ", result.Errors.Select(e => e.Description));
            return _response;
        }
        catch (Exception e)
        {
            _response.Message = e.Message;
            _response.StatusCode = HttpStatusCode.InternalServerError;
            return _response;
        }
    }

    public async Task<ResponseDto> GetUserRolesAsync(string userId)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                _response.StatusCode = HttpStatusCode.NotFound;
                _response.Message = "User not found";
                return _response;
            }

            var roles = await _userManager.GetRolesAsync(user);
            _response.Result = roles;
            _response.Message = "User roles retrieved successfully";
            return _response;
        }
        catch (Exception e)
        {
            _response.Message = e.Message;
            _response.StatusCode = HttpStatusCode.InternalServerError;
            return _response;
        }
    }

    public async Task<ResponseDto> GetAllRolesAsync()
    {
        try
        {
            var roles = await _roleManager.Roles.Select(r => r.Name).ToListAsync();
            _response.Result = roles;
            _response.Message = "Roles retrieved successfully";
            return _response;
        }
        catch (Exception e)
        {
            _response.Message = e.Message;
            _response.StatusCode = HttpStatusCode.InternalServerError;
            return _response;
        }
    }
}