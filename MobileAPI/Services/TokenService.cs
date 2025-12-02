using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using MobileAPI.DTOs.Auth;
using MobileAPI.Models;
using MobileAPI.Services.IService;

namespace MobileAPI.Services;

public class TokenService : ITokenService
{
    private readonly IConfiguration _config;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<TokenService> _logger;

    public TokenService(IConfiguration config, UserManager<ApplicationUser> userManager,  ILogger<TokenService> logger)
    {
        _config = config;
        _userManager = userManager;
        _logger = logger;
    }

    public async Task<string> CreateTokenAsync(ApplicationUser user)
    {
        var claims = new List<Claim>
        {
            new (JwtRegisteredClaimNames.Sub, user.Id),
            new (JwtRegisteredClaimNames.UniqueName, user.UserName ?? ""),
            new (JwtRegisteredClaimNames.Email, user.Email ?? "")
        };

        // add roles
        var roles = await _userManager.GetRolesAsync(user);
        foreach (var role in roles)
        {
            // claims.Add(new Claim(ClaimTypes.Role, role));
            claims.Add(new Claim("role", role));
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["JwtSettings:JwtSecret"] ?? throw new InvalidOperationException("JwtSettings:Secret missing")));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var expires = DateTime.UtcNow.AddMinutes(double.Parse(_config["JwtSettings:ExpireMinutes"] ?? "60"));

        var token = new JwtSecurityToken(
            issuer: _config["JwtSettings:JwtValidIssuer"],
            audience: _config["JwtSettings:JwtValidAudience"],
            claims: claims,
            expires: expires,
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
    
    
            public async Task<bool> ValidatePasswordResetTokenAsync(ApplicationUser user, string token)
        {
            try
            {
                // Split token and tokenId
                var parts = token.Split(':');
                if (parts.Length != 2)
                {
                    _logger.LogWarning("Invalid token format: {Token}", token);
                    return false;
                }
                
                var isValid = await _userManager.VerifyUserTokenAsync(
                    user,
                    _userManager.Options.Tokens.PasswordResetTokenProvider,
                    "ResetPassword",
                    token);
                
                if (!isValid)
                {
                    _logger.LogWarning("Token validation failed for user {UserId}", user.Id);
                }
                
                return isValid;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating password reset token");
                return false;
            }
        }
            
        public async Task<VerifyResetTokenResponseDto> VerifyResetTokenAsync(string email, string token)
        {
            var response = new VerifyResetTokenResponseDto
            {
                IsValid = false,
                Message = "Invalid token or email"
            };
            
            try
            {
                if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(token))
                {
                    response.Message = "Email and token are required";
                    return response;
                }
                
                // Find user by email
                var user = await _userManager.FindByEmailAsync(email);
                if (user == null)
                {
                    // Don't reveal that user doesn't exist for security
                    response.Message = "Invalid token";
                    return response;
                }
                
                // Verify the token with Identity
                var isValid = await _userManager.VerifyUserTokenAsync(
                    user,
                    _userManager.Options.Tokens.PasswordResetTokenProvider,
                    "ResetPassword",
                    token);
                
                if (isValid)
                {
                    response.IsValid = true;
                    response.Message = "Token is valid";
                    response.Email = user.Email;
                    // response.ExpiryDate = tokenInfo.ExpiresAt;
                    _logger.LogInformation("Reset token validated successfully for user {Email}", email);
                }
                else
                {
                    response.Message = "Invalid token";
                }
                
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying reset token for email {Email}", email);
                response.Message = "An error occurred while verifying the token";
                return response;
            }
        }
}