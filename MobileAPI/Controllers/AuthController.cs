using System.Net;
using System.Net.Mail;
using System.Text;
using System.Text.Encodings.Web;
using System.Web;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using MobileAPI.DTOs.Auth;
using MobileAPI.DTOs.Common;
using MobileAPI.Models;
using MobileAPI.Services.IService;

namespace MobileAPI.Controllers;

[ApiController]
[Route("api/[controller]")]  
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ITokenService _tokenService;
    private readonly IConfiguration _configuration;
    private readonly UrlEncoder _urlEncoder;
    private readonly ResponseDto _response;
    private readonly IRoleService _roleService;
    private readonly INotificationService _notificationService;


    public AuthController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        ITokenService tokenService,
        IConfiguration configuration,
        IRoleService roleService,
        INotificationService notificationService,
        UrlEncoder urlEncoder)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _tokenService = tokenService;
        _configuration = configuration;
        _urlEncoder = urlEncoder;
        _roleService = roleService;
        _notificationService = notificationService;
        _response = new ResponseDto();
    }

    [HttpPost("Register")]
    public async Task<ResponseDto> Register([FromBody] RegisterDto dto)
    {
        try
        {
            var user = new ApplicationUser { UserName = dto.Email.ToLower(), Email = dto.Email.ToLower(), 
                FirstName= dto.FirstName, LastName = dto.LastName, EmailConfirmed = false };

            var result = await _userManager.CreateAsync(user, dto.Password);
            if (!result.Succeeded)
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.Message = result.Errors.FirstOrDefault()?.Description ?? "Registration failed";
                return _response;
            }

            // Optionally assign default role
            // await _userManager.AddToRoleAsync(user, "User");

            // Email confirmation token
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
           
            var urlSafeToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
            // var confirmUrl = $"{Request.Scheme}://{Request.Host}/auth/confirm-email?userId={user.Id}&token={urlSafeToken}";
            var confirmUrl = $"{_configuration["ClientApp:BaseUrl"]}/auth/confirm-email?email={user.Email}&token={urlSafeToken}";
            Console.WriteLine(confirmUrl);
            
            var emailBody = new EmailDto()
            {
                From = new MailAddress("noreply@nmdpra.gov.ng", "NMDPRA-No-Reply"),
                To = new MailAddress(dto.Email),
                Subject = "Confirm Account",
                HtmlContent = confirmUrl,
            };
            await _notificationService.SendEmailNotification(emailBody); 
            
            _response.Message = "Account was created successfully. A confirmation email has been sent to your email address";
            _response.Result = confirmUrl;
            
            return _response;
        }
        catch (Exception e)
        {
            _response.Message = e.Message;
            _response.StatusCode = HttpStatusCode.InternalServerError;
            return _response;
        }
    }

    [HttpPost("confirm-email")]
    public async Task<ResponseDto> ConfirmEmail([FromBody] ConfirmEmailDto data)
    {
        try
        {
            var user = await _userManager.FindByEmailAsync(data.Email);
            if (user == null)
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.Message = "Invalid user id";
                return _response;
            }
          
            var bytes = WebEncoders.Base64UrlDecode(data.Token);
            var originalToken = Encoding.UTF8.GetString(bytes);
            
            var result = await _userManager.ConfirmEmailAsync(user, originalToken);
            if (result.Succeeded)
            {
                _response.Message = "Email confirmed successfully";
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

    [HttpPost("Login")]
    public async Task<ResponseDto> Login([FromBody] LoginDto dto)
    {
        try
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null)
            {
                _response.StatusCode = HttpStatusCode.Unauthorized;
                _response.Message = "Invalid credentials";
                return _response;
            }

            if (!await _userManager.IsEmailConfirmedAsync(user))
            {
                _response.StatusCode = HttpStatusCode.Unauthorized;
                _response.Message = "Email not confirmed";
                return _response;
            }

            var signInResult = await _signInManager.CheckPasswordSignInAsync(user, dto.Password, lockoutOnFailure: false);
            if (!signInResult.Succeeded)
            {
                _response.StatusCode = HttpStatusCode.Unauthorized;
                _response.Message = "Invalid credentials";
                return _response;
            }

            var token = await _tokenService.CreateTokenAsync(user);
            var expires = _configuration["Jwt:ExpireMinutes"] ?? "60";

            var response = new AuthResponseDto { ExpiresIn = expires, Token = token, UserId = user.Id, Email = user.Email, Name = user.FirstName + " " + user.LastName };
            _response.Result = response;
            _response.Message = "Login successful";
            return _response;
        }
        catch (Exception e)
        {
            _response.Message = e.Message;
            _response.StatusCode = HttpStatusCode.InternalServerError;
            return _response;
        }
    }

    // [HttpPost("Logout")]
    // [Authorize]
    // public async Task<ResponseDto> Logout()
    // {
    //     try
    //     {
    //         await _signInManager.SignOutAsync();
    //         _response.Message = "Logged out successfully";
    //         return _response;
    //     }
    //     catch (Exception e)
    //     {
    //         _response.Message = e.Message;
    //         _response.StatusCode = HttpStatusCode.InternalServerError;
    //         return _response;
    //     }
    // }

    [HttpPost("Forgot-Password")]
    public async Task<ResponseDto> ForgotPassword([FromBody] ForgotPasswordDto dto)
    {
        try
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
            {
                _response.Message = "If the email is registered, a password reset email will be sent.";
                return _response;
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var urlSafeToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
            var resetUrl = $"{_configuration["ClientApp:BaseUrl"]}/auth/password-change?email={user.Email}&token={urlSafeToken}";

            var emailBody = new EmailDto()
            {
                From = new MailAddress("noreply@nmdpra.gov.ng", "NMDPRA-No-Reply"),
                To = new MailAddress(dto.Email),
                Subject = "Reset Password",
                HtmlContent = resetUrl,
            };
            await _notificationService.SendEmailNotification(emailBody); 
            
            _response.Message = "Password reset link generated successfully";
            _response.Result = new { 
                ResetUrl = resetUrl,
                ResetToken = token,
            };
            Console.WriteLine(resetUrl);
            return _response;
        }
        catch (Exception e)
        {
            _response.Message = e.Message;
            _response.StatusCode = HttpStatusCode.InternalServerError;
            return _response;
        }
    }
    
    [HttpGet("Verify-Reset-Token")]
    public async Task<ResponseDto> VerifyResetToken([FromQuery] string token, [FromQuery] string email)
    {
        try
        {
            var response = new VerifyResetTokenResponseDto()
            {
                Email = email,
            };
            
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                response.IsValid = false;
                
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.Message = "Invalid token or user does not exist.";
                _response.IsSuccess = false;
                _response.Result = response;
                
                return _response;
            }
            
            var bytes = WebEncoders.Base64UrlDecode(token);
            var originalToken = Encoding.UTF8.GetString(bytes);

            var isValid = await _userManager.VerifyUserTokenAsync(
                user,
                _userManager.Options.Tokens.PasswordResetTokenProvider,
                "ResetPassword", 
                originalToken
            );

            if (!isValid)
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.IsSuccess = false;
                _response.Message = "Invalid token or user does not exist.";
                return _response;
            }
            
            response.IsValid = true;
            response.ExpiryDate = DateTime.UtcNow.AddMinutes(60);
            response.Message = "Token verified successfully";
            
            _response.Message = "Token verified successfully";
            _response.Result = response;
            
            return _response;
            
        }
        catch (Exception e)
        {
            _response.Message = e.Message;
            _response.StatusCode = HttpStatusCode.InternalServerError;
            return _response;
        }
    }
    
    
    [HttpPost("resend-confirmation-email")]
    public async Task<ResponseDto> ResendConfirmationEmail([FromBody] ResendConfirmationDto dto)
    {
        try
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null)
            {
                // Don't reveal that user doesn't exist
                _response.StatusCode = HttpStatusCode.OK;
                _response.Message = "If your email is registered, you will receive a confirmation email";
                return _response;
            }

            if (user.EmailConfirmed)
            {
                _response.StatusCode = HttpStatusCode.OK;
                _response.Message = "Email is already confirmed";
                return _response;
            }

            // Generate confirmation token
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var urlSafeToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
            // Create confirmation link
            var confirmationLink = $"{_configuration["ClientApp:BaseUrl"]}/auth/confirm-account?token={urlSafeToken}&email={user.Email}";
            
            var emailBody = new EmailDto()
            {
                From = new MailAddress("noreply@nmdpra.gov.ng", "NMDPRA-No-Reply"),
                To = new MailAddress(dto.Email),
                Subject = "Reset Password",
                HtmlContent = confirmationLink,
            };
            await _notificationService.SendEmailNotification(emailBody); 
            
        
            _response.StatusCode = HttpStatusCode.OK;
            _response.Message = "If your email is registered, you will receive a confirmation email";
            return _response;
        }
        catch (Exception e)
        {
          
            _response.Message = "An error occurred while resending confirmation email";
            _response.StatusCode = HttpStatusCode.InternalServerError;
            return _response;
        }
    }

    [HttpPost("reset-password")]
    public async Task<ResponseDto> ResetPassword([FromBody] ResetPasswordDto dto)
    {
        try
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null)
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.Message = "Invalid request";
                return _response;
            }
            
            var bytes = WebEncoders.Base64UrlDecode(dto.Token);
            var originalToken = Encoding.UTF8.GetString(bytes);
            
            var result = await _userManager.ResetPasswordAsync(user, originalToken, dto.NewPassword);
            if (result.Succeeded)
            {
                _response.Message = "Password reset successful";
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

    [HttpPost("Change-Password")]
    [Authorize(Policy = "AnyAuthenticated")]
    public async Task<ResponseDto> ChangePassword([FromBody] ChangePasswordDto dto)
    {
        try
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                _response.StatusCode = HttpStatusCode.Unauthorized;
                _response.Message = "User not found";
                return _response;
            }
            
           

            var result = await _userManager.ChangePasswordAsync(user, dto.OldPassword, dto.NewPassword);
            if (result.Succeeded)
            {
                _response.Message = "Password changed successfully";
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

    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [HttpGet("Profile")]
    public async Task<ResponseDto> Profile()
    {
        try
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                _response.StatusCode = HttpStatusCode.Unauthorized;
                _response.Message = "User not found";
                return _response;
            }

            var roles = await _userManager.GetRolesAsync(user);
            var profileData = new { user.Id, user.UserName, user.Email, Roles = roles };
            
            _response.Result = profileData;
            _response.Message = "Profile retrieved successfully";
            return _response;
        }
        catch (Exception e)
        {
            _response.Message = e.Message;
            _response.StatusCode = HttpStatusCode.InternalServerError;
            return _response;
        }
    }
    
    //Role Services
    [HttpPost("CreateRole")]
    [Authorize(Roles = "Admin", AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<ResponseDto> CreateRole([FromBody] CreateRoleDto dto)
    {
        return await _roleService.CreateRoleAsync(dto.RoleName);
    }

    [HttpDelete("DeleteRole/{roleName}")]
    [Authorize(Roles = "Admin", AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<ResponseDto> DeleteRole(string roleName)
    {
        return await _roleService.DeleteRoleAsync(roleName);
    }

    [HttpPost("AssignRole")]
    [Authorize(Roles = "Admin",  AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<ResponseDto> AssignRoleToUser([FromBody] AssignRoleDto dto)
    {
        return await _roleService.AssignRoleToUserAsync(dto.Email, dto.RoleName);
    }

    [HttpPost("RemoveRole")]
    [Authorize(Roles = "Admin", AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<ResponseDto> RemoveRoleFromUser([FromBody] AssignRoleDto dto)
    {
        return await _roleService.RemoveRoleFromUserAsync(dto.Email, dto.RoleName);
    }

    [HttpGet("UserRoles/{email}")]
    [Authorize(Roles = "Admin", AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<ResponseDto> GetUserRoles(string email)
    {
        return await _roleService.GetUserRolesAsync(email);
    }

    [HttpGet("AllRoles")]
    [Authorize(Roles = "Admin", AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<ResponseDto> GetAllRoles()
    {
        return await _roleService.GetAllRolesAsync();
    }
}

