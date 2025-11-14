using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using LabResultsApi.Data;
using LabResultsApi.DTOs;
using LabResultsApi.Models;

namespace LabResultsApi.Services;

public class AuthenticationService : IAuthenticationService
{
    private readonly LabDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthenticationService> _logger;

    public AuthenticationService(LabDbContext context, IConfiguration configuration, ILogger<AuthenticationService> logger)
    {
        _context = context;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<LoginResponseDto?> AuthenticateAsync(LoginRequestDto loginRequest)
    {
        try
        {
            // First try to authenticate as a LubeTech
            var lubeTech = await _context.LubeTechs
                .FirstOrDefaultAsync(lt => lt.EmployeeId == loginRequest.EmployeeId);

            if (lubeTech != null && lubeTech.QualificationPassword == loginRequest.Password)
            {
                // Get qualifications using raw SQL since LubeTechQualification is keyless
                var qualifications = await _context.LubeTechQualifications
                    .FromSqlRaw("SELECT * FROM LubeTechQualification WHERE employeeID = {0}", lubeTech.EmployeeId)
                    .ToListAsync();

                var token = GenerateJwtToken(lubeTech, qualifications);
                var qualificationDtos = qualifications.Select(q => new DTOs.UserQualificationDto
                {
                    TestStandId = q.TestStandId,
                    TestStand = q.TestStand,
                    QualificationLevel = q.QualificationLevel
                }).ToList();

                return new LoginResponseDto
                {
                    Token = token,
                    EmployeeId = lubeTech.EmployeeId,
                    FullName = lubeTech.FullName,
                    Role = "Technician",
                    Qualifications = qualificationDtos,
                    ExpiresAt = DateTime.UtcNow.AddHours(8)
                };
            }

            // If not found as LubeTech, try as Reviewer
            var reviewer = await _context.Reviewers
                .FirstOrDefaultAsync(r => r.EmployeeId == loginRequest.EmployeeId);

            if (reviewer != null && reviewer.ReviewerPassword == loginRequest.Password)
            {
                var token = GenerateJwtToken(reviewer);
                
                return new LoginResponseDto
                {
                    Token = token,
                    EmployeeId = reviewer.EmployeeId,
                    FullName = reviewer.FullName,
                    Role = "Reviewer",
                    Qualifications = new List<DTOs.UserQualificationDto>(), // Reviewers don't have test-specific qualifications
                    ExpiresAt = DateTime.UtcNow.AddHours(8)
                };
            }

            _logger.LogWarning("Authentication failed for employee ID: {EmployeeId}", loginRequest.EmployeeId);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during authentication for employee ID: {EmployeeId}", loginRequest.EmployeeId);
            return null;
        }
    }

    public async Task<UserInfoDto?> GetUserInfoAsync(string employeeId)
    {
        try
        {
            // First check LubeTech
            var lubeTech = await _context.LubeTechs
                .FirstOrDefaultAsync(lt => lt.EmployeeId == employeeId);

            if (lubeTech != null)
            {
                // Get qualifications using raw SQL since LubeTechQualification is keyless
                var qualificationEntities = await _context.LubeTechQualifications
                    .FromSqlRaw("SELECT * FROM LubeTechQualification WHERE employeeID = {0}", lubeTech.EmployeeId)
                    .ToListAsync();

                var qualifications = qualificationEntities.Select(q => new DTOs.UserQualificationDto
                {
                    TestStandId = q.TestStandId,
                    TestStand = q.TestStand,
                    QualificationLevel = q.QualificationLevel
                }).ToList();

                return new UserInfoDto
                {
                    EmployeeId = lubeTech.EmployeeId,
                    FullName = lubeTech.FullName,
                    Role = "Technician",
                    Qualifications = qualifications
                };
            }

            // Check Reviewer
            var reviewer = await _context.Reviewers
                .FirstOrDefaultAsync(r => r.EmployeeId == employeeId);

            if (reviewer != null)
            {
                return new UserInfoDto
                {
                    EmployeeId = reviewer.EmployeeId,
                    FullName = reviewer.FullName,
                    Role = "Reviewer",
                    Qualifications = new List<DTOs.UserQualificationDto>()
                };
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user info for employee ID: {EmployeeId}", employeeId);
            return null;
        }
    }

    public async Task<TestAccessResponseDto> CheckTestAccessAsync(string employeeId, short testStandId, string requiredLevel)
    {
        try
        {
            // Use raw SQL since LubeTechQualification is keyless
            var qualification = await _context.LubeTechQualifications
                .FromSqlRaw("SELECT * FROM LubeTechQualification WHERE employeeID = {0} AND testStandID = {1}", 
                           employeeId, testStandId)
                .FirstOrDefaultAsync();

            if (qualification == null)
            {
                return new TestAccessResponseDto
                {
                    HasAccess = false,
                    RequiredLevel = requiredLevel,
                    Message = "No qualification found for this test"
                };
            }

            var userLevel = QualificationLevelExtensions.ParseQualificationLevel(qualification.QualificationLevel);
            var requiredLevelEnum = QualificationLevelExtensions.ParseQualificationLevel(requiredLevel);

            // Check if user has sufficient qualification level
            // MicrE > Q/QAG > TRAIN
            bool hasAccess = userLevel switch
            {
                QualificationLevel.MicrE => true, // MicrE can do everything
                QualificationLevel.Q_QAG => requiredLevelEnum != QualificationLevel.MicrE, // Q/QAG can do Q/QAG and TRAIN
                QualificationLevel.TRAIN => requiredLevelEnum == QualificationLevel.TRAIN, // TRAIN can only do TRAIN
                _ => false
            };

            return new TestAccessResponseDto
            {
                HasAccess = hasAccess,
                UserQualificationLevel = qualification.QualificationLevel,
                RequiredLevel = requiredLevel,
                Message = hasAccess ? "Access granted" : $"Insufficient qualification level. Required: {requiredLevel}, User has: {qualification.QualificationLevel}"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking test access for employee ID: {EmployeeId}, Test Stand ID: {TestStandId}", employeeId, testStandId);
            return new TestAccessResponseDto
            {
                HasAccess = false,
                RequiredLevel = requiredLevel,
                Message = "Error checking access permissions"
            };
        }
    }

    public async Task<List<DTOs.UserQualificationDto>> GetUserQualificationsAsync(string employeeId)
    {
        try
        {
            // Use raw SQL since LubeTechQualification is keyless
            var qualifications = await _context.LubeTechQualifications
                .FromSqlRaw("SELECT * FROM LubeTechQualification WHERE employeeID = {0}", employeeId)
                .ToListAsync();

            return qualifications.Select(q => new DTOs.UserQualificationDto
            {
                TestStandId = q.TestStandId,
                TestStand = q.TestStand,
                QualificationLevel = q.QualificationLevel
            }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting qualifications for employee ID: {EmployeeId}", employeeId);
            return new List<DTOs.UserQualificationDto>();
        }
    }

    public string GenerateJwtToken(LubeTech user, List<LubeTechQualification> qualifications)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"] ?? "your-secret-key-here-make-it-long-enough");
        
        var now = DateTime.UtcNow;
        var expirationHours = _configuration.GetValue<int>("Jwt:ExpirationHours", 8);
        
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.EmployeeId),
            new(ClaimTypes.Name, user.FullName),
            new(ClaimTypes.Role, "Technician"),
            new("employee_id", user.EmployeeId),
            new("user_type", "LubeTech"),
            new("iat", new DateTimeOffset(now).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
            new("jti", Guid.NewGuid().ToString()) // JWT ID for token tracking
        };

        // Add qualification claims
        foreach (var qualification in qualifications)
        {
            if (qualification.TestStandId.HasValue)
            {
                claims.Add(new Claim($"qualification_{qualification.TestStandId}", qualification.QualificationLevel ?? "TRAIN"));
            }
        }

        // Add highest qualification level as a general claim
        var highestLevel = GetHighestQualificationLevel(qualifications);
        claims.Add(new Claim("highest_qualification", highestLevel.ToDisplayString()));

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = now.AddHours(expirationHours),
            NotBefore = now,
            IssuedAt = now,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
            Issuer = _configuration["Jwt:Issuer"],
            Audience = _configuration["Jwt:Audience"]
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    public string GenerateJwtToken(Reviewer reviewer)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"] ?? "your-secret-key-here-make-it-long-enough");
        
        var now = DateTime.UtcNow;
        var expirationHours = _configuration.GetValue<int>("Jwt:ExpirationHours", 8);
        
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, reviewer.EmployeeId),
            new(ClaimTypes.Name, reviewer.FullName),
            new(ClaimTypes.Role, "Reviewer"),
            new("employee_id", reviewer.EmployeeId),
            new("user_type", "Reviewer"),
            new("reviewer_level", reviewer.Level.ToString()),
            new("iat", new DateTimeOffset(now).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
            new("jti", Guid.NewGuid().ToString()) // JWT ID for token tracking
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = now.AddHours(expirationHours),
            NotBefore = now,
            IssuedAt = now,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
            Issuer = _configuration["Jwt:Issuer"],
            Audience = _configuration["Jwt:Audience"]
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    public bool ValidateToken(string token)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                _logger.LogWarning("Token validation failed: Token is null or empty");
                return false;
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"] ?? "your-secret-key-here-make-it-long-enough");
            
            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = _configuration["Jwt:Issuer"],
                ValidateAudience = true,
                ValidAudience = _configuration["Jwt:Audience"],
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero,
                RequireExpirationTime = true,
                RequireSignedTokens = true
            }, out SecurityToken validatedToken);

            // Additional validation - ensure token is JWT
            if (validatedToken is not JwtSecurityToken jwtToken)
            {
                _logger.LogWarning("Token validation failed: Token is not a valid JWT");
                return false;
            }

            // Validate algorithm
            if (!jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                _logger.LogWarning("Token validation failed: Invalid algorithm {Algorithm}", jwtToken.Header.Alg);
                return false;
            }

            return true;
        }
        catch (SecurityTokenExpiredException ex)
        {
            _logger.LogWarning("Token validation failed: Token expired - {Message}", ex.Message);
            return false;
        }
        catch (SecurityTokenInvalidSignatureException ex)
        {
            _logger.LogWarning("Token validation failed: Invalid signature - {Message}", ex.Message);
            return false;
        }
        catch (SecurityTokenValidationException ex)
        {
            _logger.LogWarning("Token validation failed: {Message}", ex.Message);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during token validation");
            return false;
        }
    }

    public string? GetEmployeeIdFromToken(string token)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(token))
                return null;

            var tokenHandler = new JwtSecurityTokenHandler();
            var jsonToken = tokenHandler.ReadJwtToken(token);
            
            return jsonToken.Claims?.FirstOrDefault(x => x.Type == "employee_id")?.Value;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error extracting employee ID from token");
            return null;
        }
    }

    private QualificationLevel GetHighestQualificationLevel(List<LubeTechQualification> qualifications)
    {
        var highestLevel = QualificationLevel.TRAIN;
        
        foreach (var qualification in qualifications)
        {
            var level = QualificationLevelExtensions.ParseQualificationLevel(qualification.QualificationLevel);
            if (level > highestLevel)
            {
                highestLevel = level;
            }
        }
        
        return highestLevel;
    }
}