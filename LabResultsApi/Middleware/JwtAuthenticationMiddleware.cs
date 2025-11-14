using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace LabResultsApi.Middleware;

public class JwtAuthenticationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IConfiguration _configuration;
    private readonly ILogger<JwtAuthenticationMiddleware> _logger;

    public JwtAuthenticationMiddleware(RequestDelegate next, IConfiguration configuration, ILogger<JwtAuthenticationMiddleware> logger)
    {
        _next = next;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var token = ExtractTokenFromHeader(context);

        if (!string.IsNullOrEmpty(token))
        {
            try
            {
                var principal = ValidateToken(token);
                if (principal != null)
                {
                    context.User = principal;
                    
                    // Add token info to context for audit purposes
                    var employeeId = principal.FindFirst("employee_id")?.Value;
                    var jti = principal.FindFirst("jti")?.Value;
                    
                    if (!string.IsNullOrEmpty(employeeId))
                    {
                        context.Items["EmployeeId"] = employeeId;
                    }
                    if (!string.IsNullOrEmpty(jti))
                    {
                        context.Items["TokenId"] = jti;
                    }
                }
                else
                {
                    _logger.LogWarning("Token validation returned null principal");
                }
            }
            catch (SecurityTokenExpiredException ex)
            {
                _logger.LogWarning("JWT token expired: {Message}", ex.Message);
                context.Response.Headers["Token-Expired"] = "true";
            }
            catch (SecurityTokenInvalidSignatureException ex)
            {
                _logger.LogWarning("JWT token has invalid signature: {Message}", ex.Message);
                context.Response.Headers["Token-Invalid"] = "signature";
            }
            catch (SecurityTokenValidationException ex)
            {
                _logger.LogWarning("JWT token validation failed: {Message}", ex.Message);
                context.Response.Headers["Token-Invalid"] = "validation";
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Unexpected error validating JWT token");
                context.Response.Headers["Token-Invalid"] = "error";
            }
        }

        await _next(context);
    }

    private string? ExtractTokenFromHeader(HttpContext context)
    {
        var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
        
        if (authHeader != null && authHeader.StartsWith("Bearer "))
        {
            return authHeader.Substring("Bearer ".Length).Trim();
        }

        return null;
    }

    private ClaimsPrincipal? ValidateToken(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"] ?? "your-secret-key-here-make-it-long-enough");

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = _configuration["Jwt:Issuer"],
                ValidateAudience = true,
                ValidAudience = _configuration["Jwt:Audience"],
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            var principal = tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);
            return principal;
        }
        catch
        {
            return null;
        }
    }
}