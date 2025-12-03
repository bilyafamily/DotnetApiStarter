using System.Net;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MobileAPI.DTOs.Common;
using MobileAPI.DTOs.UserDtos;
using MobileAPI.Models;
using MobileAPI.Services.IService;

namespace MobileAPI.Services;

public class UserManagementService : IUserManagementService
{
     private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<UserManagementService> _logger;
        private readonly ResponseDto _response;

        public UserManagementService(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ILogger<UserManagementService> logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
            _response = new ResponseDto();
        }

        public async Task<ResponseDto> GetAllUsersAsync()
        {
            try
            {
                var users = await _userManager.Users
                    .OrderBy(u => u.FirstName)
                    .ThenBy(u => u.LastName)
                    .ToListAsync();

                var userDtos = new List<UserDto>();

                foreach (var user in users)
                {
                    var roles = await _userManager.GetRolesAsync(user);
                    userDtos.Add(new UserDto
                    {
                        Id = user.Id,
                        Email = user.Email!,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        PhoneNumber = user.PhoneNumber,
                        IsActive = user.IsActive,
                        EmailConfirmed = user.EmailConfirmed,
                        CreatedAt = user.CreatedAt,
                        Roles = roles.ToList()
                    });
                }

                _response.Result = userDtos;
                _response.Message = "Users retrieved successfully";
                return _response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all users");
                _response.StatusCode = HttpStatusCode.InternalServerError;
                _response.Message = "An error occurred while retrieving users";
                return _response;
            }
        }

        public async Task<ResponseDto> GetUserByIdAsync(string userId)
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
                var userDto = new UserDto
                {
                    Id = user.Id,
                    Email = user.Email!,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    PhoneNumber = user.PhoneNumber,
                    IsActive = user.IsActive,
                    EmailConfirmed = user.EmailConfirmed,
                    CreatedAt = user.CreatedAt,
                    Roles = roles.ToList()
                };

                _response.Result = userDto;
                _response.Message = "User retrieved successfully";
                return _response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user by ID: {UserId}", userId);
                _response.StatusCode = HttpStatusCode.InternalServerError;
                _response.Message = "An error occurred while retrieving user";
                return _response;
            }
        }

        public async Task<ResponseDto> CreateUserAsync(CreateUserDto dto)
        {
            try
            {
                // Check if user already exists
                var existingUser = await _userManager.FindByEmailAsync(dto.Email);
                if (existingUser != null)
                {
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    _response.Message = "User with this email already exists";
                    _response.IsSuccess = false;
                    return _response;
                }

                // Validate roles
                foreach (var roleName in dto.Roles)
                {
                    if (!await _roleManager.RoleExistsAsync(roleName))
                    {
                        _response.StatusCode = HttpStatusCode.BadRequest;
                        _response.Message = $"Role '{roleName}' does not exist";
                        return _response;
                    }
                }

                // Create user
                var user = new ApplicationUser
                {
                    UserName = dto.Email,
                    Email = dto.Email,
                    FirstName = dto.FirstName,
                    LastName = dto.LastName,
                    PhoneNumber = dto.PhoneNumber,
                    IsActive = true,
                    EmailConfirmed = false,
                    CreatedAt = DateTime.UtcNow
                };

                var result = await _userManager.CreateAsync(user, dto.Password);
                if (!result.Succeeded)
                {
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    _response.Message = string.Join(", ", result.Errors.Select(e => e.Description));
                    return _response;
                }

                // Assign roles
                if (dto.Roles.Any())
                {
                    var roleResult = await _userManager.AddToRolesAsync(user, dto.Roles);
                    if (!roleResult.Succeeded)
                    {
                        _logger.LogWarning("Failed to assign roles to user {Email}: {Errors}", 
                            user.Email, string.Join(", ", roleResult.Errors.Select(e => e.Description)));
                        // Continue even if role assignment fails
                    }
                }

                // Get user with roles for response
                var roles = await _userManager.GetRolesAsync(user);
                var userDto = new UserDto
                {
                    Id = user.Id,
                    Email = user.Email!,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    PhoneNumber = user.PhoneNumber,
                    IsActive = user.IsActive,
                    EmailConfirmed = user.EmailConfirmed,
                    CreatedAt = user.CreatedAt,
                    Roles = roles.ToList()
                };

                _logger.LogInformation("User created successfully: {Email}", user.Email);
                _response.Result = userDto;
                _response.Message = "User created successfully";
                return _response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user with email: {Email}", dto.Email);
                _response.StatusCode = HttpStatusCode.InternalServerError;
                _response.Message = "An error occurred while creating user";
                return _response;
            }
        }

        public async Task<ResponseDto> UpdateUserAsync(string userId, UpdateUserDto dto)
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

                // Update properties if provided
                if (!string.IsNullOrWhiteSpace(dto.FirstName))
                    user.FirstName = dto.FirstName;
                
                if (!string.IsNullOrWhiteSpace(dto.LastName))
                    user.LastName = dto.LastName;
                
                if (dto.PhoneNumber != null)
                    user.PhoneNumber = dto.PhoneNumber;
                
                if (dto.IsActive.HasValue)
                    user.IsActive = dto.IsActive.Value;

                var result = await _userManager.UpdateAsync(user);
                if (!result.Succeeded)
                {
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    _response.Message = string.Join(", ", result.Errors.Select(e => e.Description));
                    return _response;
                }

                // Get updated user with roles
                var roles = await _userManager.GetRolesAsync(user);
                var userDto = new UserDto
                {
                    Id = user.Id,
                    Email = user.Email!,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    PhoneNumber = user.PhoneNumber,
                    IsActive = user.IsActive,
                    EmailConfirmed = user.EmailConfirmed,
                    CreatedAt = user.CreatedAt,
                    Roles = roles.ToList()
                };

                _logger.LogInformation("User updated successfully: {UserId}", userId);
                _response.Result = userDto;
                _response.Message = "User updated successfully";
                return _response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user: {UserId}", userId);
                _response.StatusCode = HttpStatusCode.InternalServerError;
                _response.Message = "An error occurred while updating user";
                return _response;
            }
        }

        public async Task<ResponseDto> ToggleUserStatusAsync(string userId, bool isActive)
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

                user.IsActive = isActive;
                var result = await _userManager.UpdateAsync(user);

                if (!result.Succeeded)
                {
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    _response.Message = string.Join(", ", result.Errors.Select(e => e.Description));
                    return _response;
                }

                _logger.LogInformation("User status changed to {Status}: {UserId}", 
                    isActive ? "Active" : "Inactive", userId);
                
                _response.Message = $"User {(isActive ? "enabled" : "disabled")} successfully";
                _response.Result = user;
                return _response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling user status: {UserId}", userId);
                _response.StatusCode = HttpStatusCode.InternalServerError;
                _response.Message = "An error occurred while updating user status";
                return _response;
            }
        }

        public async Task<ResponseDto> ResetPasswordAsync(ResetUserPasswordDto dto)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(dto.UserId);
                if (user == null)
                {
                    _response.StatusCode = HttpStatusCode.NotFound;
                    _response.Message = "User not found";
                    return _response;
                }

                // Generate reset token
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                
                // Reset password
                var result = await _userManager.ResetPasswordAsync(user, token, dto.NewPassword);
                if (!result.Succeeded)
                {
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    _response.Message = string.Join(", ", result.Errors.Select(e => e.Description));
                    return _response;
                }

                _logger.LogInformation("Password reset successfully for user: {UserId}", dto.UserId);
                _response.Message = "Password reset successfully";
                return _response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting password for user: {UserId}", dto.UserId);
                _response.StatusCode = HttpStatusCode.InternalServerError;
                _response.Message = "An error occurred while resetting password";
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user roles: {UserId}", userId);
                _response.StatusCode = HttpStatusCode.InternalServerError;
                _response.Message = "An error occurred while retrieving user roles";
                return _response;
            }
        }

        public async Task<ResponseDto> GetAllRolesWithCountAsync()
        {
            try
            {
                var roles = await _roleManager.Roles.ToListAsync();
                var roleDtos = new List<RoleDto>();

                foreach (var role in roles)
                {
                    var userCount = await _userManager.GetUsersInRoleAsync(role.Name!);
                    roleDtos.Add(new RoleDto
                    {
                        Name = role.Name!,
                        UserCount = userCount.Count
                    });
                }

                _response.Result = roleDtos;
                _response.Message = "Roles retrieved successfully";
                return _response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all roles");
                _response.StatusCode = HttpStatusCode.InternalServerError;
                _response.Message = "An error occurred while retrieving roles";
                return _response;
            }
        }
    }
