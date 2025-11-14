using System.ComponentModel.DataAnnotations;

namespace LabResultsApi.DTOs;

public class LoginRequestDto
{
    [Required]
    [StringLength(5)]
    public string EmployeeId { get; set; } = string.Empty;

    [Required]
    [StringLength(8)]
    public string Password { get; set; } = string.Empty;
}

public class LoginResponseDto
{
    public string Token { get; set; } = string.Empty;
    public string EmployeeId { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public List<UserQualificationDto> Qualifications { get; set; } = new();
    public DateTime ExpiresAt { get; set; }
}

public class UserQualificationDto
{
    public short? TestStandId { get; set; }
    public string? TestStand { get; set; }
    public string? QualificationLevel { get; set; }
}

public class UserInfoDto
{
    public string EmployeeId { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public List<UserQualificationDto> Qualifications { get; set; } = new();
}

public class AuthorizeTestAccessDto
{
    public string EmployeeId { get; set; } = string.Empty;
    public short TestStandId { get; set; }
    public string RequiredLevel { get; set; } = string.Empty;
}

public class TestAccessResponseDto
{
    public bool HasAccess { get; set; }
    public string? UserQualificationLevel { get; set; }
    public string RequiredLevel { get; set; } = string.Empty;
    public string? Message { get; set; }
}

public class AuditLogDto
{
    public int Id { get; set; }
    public string EmployeeId { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public string? EntityId { get; set; }
    public DateTime Timestamp { get; set; }
    public string? IpAddress { get; set; }
    public string? AdditionalInfo { get; set; }
}