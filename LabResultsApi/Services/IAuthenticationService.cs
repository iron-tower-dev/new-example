using LabResultsApi.DTOs;
using LabResultsApi.Models;

namespace LabResultsApi.Services;

public interface IAuthenticationService
{
    Task<LoginResponseDto?> AuthenticateAsync(LoginRequestDto loginRequest);
    Task<UserInfoDto?> GetUserInfoAsync(string employeeId);
    Task<TestAccessResponseDto> CheckTestAccessAsync(string employeeId, short testStandId, string requiredLevel);
    Task<List<UserQualificationDto>> GetUserQualificationsAsync(string employeeId);
    string GenerateJwtToken(LubeTech user, List<LubeTechQualification> qualifications);
    string GenerateJwtToken(Reviewer reviewer);
    bool ValidateToken(string token);
    string? GetEmployeeIdFromToken(string token);
}