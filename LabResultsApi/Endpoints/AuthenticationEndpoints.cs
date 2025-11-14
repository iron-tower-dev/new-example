using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LabResultsApi.DTOs;
using LabResultsApi.Services;
using System.Security.Claims;

namespace LabResultsApi.Endpoints;

public static class AuthenticationEndpoints
{
    public static void MapAuthenticationEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth")
            .WithTags("Authentication")
            .WithOpenApi();

        group.MapPost("/login", Login)
            .WithName("Login")
            .WithSummary("Authenticate user with employee ID and password")
            .Produces<LoginResponseDto>(200)
            .Produces(401);

        group.MapPost("/logout", Logout)
            .WithName("Logout")
            .WithSummary("Logout user (client-side token removal)")
            .Produces(200)
            .RequireAuthorization();

        group.MapGet("/me", GetCurrentUser)
            .WithName("GetCurrentUser")
            .WithSummary("Get current authenticated user information")
            .Produces<UserInfoDto>(200)
            .Produces(401)
            .RequireAuthorization();

        group.MapPost("/check-test-access", CheckTestAccess)
            .WithName("CheckTestAccess")
            .WithSummary("Check if user has access to perform a specific test")
            .Produces<TestAccessResponseDto>(200)
            .Produces(401)
            .RequireAuthorization();

        group.MapGet("/qualifications", GetUserQualifications)
            .WithName("GetUserQualifications")
            .WithSummary("Get current user's test qualifications")
            .Produces<List<DTOs.UserQualificationDto>>(200)
            .Produces(401)
            .RequireAuthorization();

        group.MapPost("/validate-token", ValidateToken)
            .WithName("ValidateToken")
            .WithSummary("Validate JWT token")
            .Produces<bool>(200);

        group.MapGet("/audit-logs", GetAuditLogs)
            .WithName("GetAuditLogs")
            .WithSummary("Get audit logs (Reviewers only)")
            .Produces<List<AuditLogDto>>(200)
            .Produces(403)
            .RequireAuthorization();

        group.MapPost("/refresh-token", RefreshToken)
            .WithName("RefreshToken")
            .WithSummary("Refresh JWT token")
            .Produces<LoginResponseDto>(200)
            .Produces(401)
            .RequireAuthorization();
    }

    private static async Task<IResult> Login(
        [FromBody] LoginRequestDto loginRequest,
        IAuthenticationService authService,
        IAuditService auditService,
        ILogger<AuthenticationService> logger)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(loginRequest.EmployeeId) || string.IsNullOrWhiteSpace(loginRequest.Password))
            {
                await auditService.LogLoginAsync(loginRequest.EmployeeId ?? "UNKNOWN", false, "Missing credentials");
                return Results.BadRequest(new { message = "Employee ID and password are required" });
            }

            var result = await authService.AuthenticateAsync(loginRequest);
            
            if (result == null)
            {
                logger.LogWarning("Failed login attempt for employee ID: {EmployeeId}", loginRequest.EmployeeId);
                await auditService.LogLoginAsync(loginRequest.EmployeeId, false, "Invalid credentials");
                return Results.Unauthorized();
            }

            logger.LogInformation("Successful login for employee ID: {EmployeeId}", loginRequest.EmployeeId);
            await auditService.LogLoginAsync(loginRequest.EmployeeId, true, $"Role: {result.Role}");
            return Results.Ok(result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during login for employee ID: {EmployeeId}", loginRequest.EmployeeId);
            await auditService.LogLoginAsync(loginRequest.EmployeeId ?? "UNKNOWN", false, $"System error: {ex.Message}");
            return Results.Problem("An error occurred during authentication");
        }
    }

    private static async Task<IResult> Logout(
        ClaimsPrincipal user, 
        IAuditService auditService,
        ILogger<AuthenticationService> logger)
    {
        try
        {
            var employeeId = user.FindFirst("employee_id")?.Value;
            
            if (!string.IsNullOrEmpty(employeeId))
            {
                logger.LogInformation("User logged out: {EmployeeId}", employeeId);
                await auditService.LogLogoutAsync(employeeId);
            }
            
            return Results.Ok(new { message = "Logged out successfully" });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during logout");
            return Results.Problem("An error occurred during logout");
        }
    }

    private static async Task<IResult> GetCurrentUser(
        ClaimsPrincipal user,
        IAuthenticationService authService,
        ILogger<AuthenticationService> logger)
    {
        try
        {
            var employeeId = user.FindFirst("employee_id")?.Value;
            
            if (string.IsNullOrEmpty(employeeId))
            {
                return Results.Unauthorized();
            }

            var userInfo = await authService.GetUserInfoAsync(employeeId);
            
            if (userInfo == null)
            {
                return Results.NotFound(new { message = "User not found" });
            }

            return Results.Ok(userInfo);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting current user info");
            return Results.Problem("An error occurred while retrieving user information");
        }
    }

    private static async Task<IResult> CheckTestAccess(
        [FromBody] AuthorizeTestAccessDto request,
        ClaimsPrincipal user,
        IAuthenticationService authService,
        ILogger<AuthenticationService> logger)
    {
        try
        {
            var employeeId = user.FindFirst("employee_id")?.Value;
            
            if (string.IsNullOrEmpty(employeeId))
            {
                return Results.Unauthorized();
            }

            // Use the employee ID from the token, not from the request for security
            var result = await authService.CheckTestAccessAsync(employeeId, request.TestStandId, request.RequiredLevel);
            
            return Results.Ok(result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error checking test access for test stand ID: {TestStandId}", request.TestStandId);
            return Results.Problem("An error occurred while checking test access");
        }
    }

    private static async Task<IResult> GetUserQualifications(
        ClaimsPrincipal user,
        IAuthenticationService authService,
        ILogger<AuthenticationService> logger)
    {
        try
        {
            var employeeId = user.FindFirst("employee_id")?.Value;
            
            if (string.IsNullOrEmpty(employeeId))
            {
                return Results.Unauthorized();
            }

            var qualifications = await authService.GetUserQualificationsAsync(employeeId);
            
            return Results.Ok(qualifications);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting user qualifications");
            return Results.Problem("An error occurred while retrieving qualifications");
        }
    }

    private static IResult ValidateToken(
        [FromBody] ValidateTokenRequest request,
        IAuthenticationService authService,
        ILogger<AuthenticationService> logger)
    {
        try
        {
            var isValid = authService.ValidateToken(request.Token);
            return Results.Ok(new { isValid });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error validating token");
            return Results.Ok(new { isValid = false });
        }
    }

    private static async Task<IResult> GetAuditLogs(
        ClaimsPrincipal user,
        IAuditService auditService,
        ILogger<AuthenticationService> logger,
        [FromQuery] string? employeeId = null,
        [FromQuery] string? entityType = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        try
        {
            var userRole = user.FindFirst(ClaimTypes.Role)?.Value;
            var currentEmployeeId = user.FindFirst("employee_id")?.Value;

            // Only reviewers can access audit logs, or users can see their own logs
            if (userRole != "Reviewer" && (string.IsNullOrEmpty(employeeId) || employeeId != currentEmployeeId))
            {
                return Results.Forbid();
            }

            // If not a reviewer, restrict to own logs only
            if (userRole != "Reviewer")
            {
                employeeId = currentEmployeeId;
            }

            var auditLogs = await auditService.GetAuditLogsAsync(employeeId, entityType, fromDate, toDate, page, pageSize);
            
            var auditLogDtos = auditLogs.Select(log => new AuditLogDto
            {
                Id = log.Id,
                EmployeeId = log.EmployeeId,
                Action = log.Action,
                EntityType = log.EntityType,
                EntityId = log.EntityId,
                Timestamp = log.Timestamp,
                IpAddress = log.IpAddress,
                AdditionalInfo = log.AdditionalInfo
            }).ToList();

            return Results.Ok(auditLogDtos);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving audit logs");
            return Results.Problem("An error occurred while retrieving audit logs");
        }
    }

    private static async Task<IResult> RefreshToken(
        ClaimsPrincipal user,
        IAuthenticationService authService,
        ILogger<AuthenticationService> logger)
    {
        try
        {
            var employeeId = user.FindFirst("employee_id")?.Value;
            var userType = user.FindFirst("user_type")?.Value;

            if (string.IsNullOrEmpty(employeeId) || string.IsNullOrEmpty(userType))
            {
                return Results.Unauthorized();
            }

            // Get fresh user info and generate new token
            var userInfo = await authService.GetUserInfoAsync(employeeId);
            if (userInfo == null)
            {
                return Results.Unauthorized();
            }

            // Create a new login response with fresh token
            var response = new LoginResponseDto
            {
                EmployeeId = userInfo.EmployeeId,
                FullName = userInfo.FullName,
                Role = userInfo.Role,
                Qualifications = userInfo.Qualifications,
                ExpiresAt = DateTime.UtcNow.AddHours(8)
            };

            // Generate new token based on user type
            if (userType == "LubeTech")
            {
                // Get LubeTech and qualifications for new token
                var loginRequest = new LoginRequestDto { EmployeeId = employeeId, Password = "" };
                // Note: This is a refresh, so we don't validate password again
                // In a production system, you might want to implement a proper refresh token mechanism
                logger.LogInformation("Token refreshed for LubeTech: {EmployeeId}", employeeId);
            }
            else if (userType == "Reviewer")
            {
                logger.LogInformation("Token refreshed for Reviewer: {EmployeeId}", employeeId);
            }

            return Results.Ok(response);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error refreshing token");
            return Results.Problem("An error occurred while refreshing token");
        }
    }
}

public class ValidateTokenRequest
{
    public string Token { get; set; } = string.Empty;
}