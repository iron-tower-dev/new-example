namespace LabResultsApi.DTOs;

public class EquipmentDto
{
    public int Id { get; set; }
    public string EquipType { get; set; } = string.Empty;
    public string? EquipName { get; set; }
    public bool? Exclude { get; set; }
    public short? TestId { get; set; }
    public DateTime? DueDate { get; set; }
    public string? Comments { get; set; }
    public double? Val1 { get; set; }
    public double? Val2 { get; set; }
    public double? Val3 { get; set; }
    public double? Val4 { get; set; }
}

public class EquipmentSelectionDto
{
    public int Id { get; set; }
    public string EquipName { get; set; } = string.Empty;
    public string EquipType { get; set; } = string.Empty;
    public DateTime? DueDate { get; set; }
    public bool IsDueSoon { get; set; }
    public bool IsOverdue { get; set; }
    public double? CalibrationValue { get; set; }
    public string DisplayText { get; set; } = string.Empty;
    public bool IsValid { get; set; } = true;
    public bool IsDisabled { get; set; }
    public string? ValidationMessage { get; set; }
}

public class EquipmentCalibrationDto
{
    public int EquipmentId { get; set; }
    public string EquipName { get; set; } = string.Empty;
    public string EquipType { get; set; } = string.Empty;
    public double? CalibrationValue { get; set; }
    public DateTime? DueDate { get; set; }
    public bool IsValid { get; set; }
    public string? ValidationMessage { get; set; }
}

public class EquipmentFilterDto
{
    public string? EquipType { get; set; }
    public short? TestId { get; set; }
    public string? LubeType { get; set; }
    public bool ExcludeInactive { get; set; } = true;
    public bool IncludeOverdue { get; set; } = false;
}

public class EquipmentValidationResult
{
    public bool IsValid { get; set; }
    public bool IsDueSoon { get; set; }
    public bool IsOverdue { get; set; }
    public string? Message { get; set; }
    public DateTime? DueDate { get; set; }
    public int DaysUntilDue { get; set; }
}