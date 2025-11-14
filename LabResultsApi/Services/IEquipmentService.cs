using LabResultsApi.DTOs;

namespace LabResultsApi.Services;

public interface IEquipmentService
{
    Task<List<EquipmentSelectionDto>> GetEquipmentByTypeAsync(string equipType, short? testId = null, string? lubeType = null);
    Task<EquipmentCalibrationDto?> GetEquipmentCalibrationAsync(int equipmentId);
    Task<EquipmentValidationResult> ValidateEquipmentAsync(int equipmentId);
    Task<List<EquipmentDto>> GetAllEquipmentAsync(EquipmentFilterDto? filter = null);
    Task<EquipmentDto?> GetEquipmentByIdAsync(int id);
    Task<bool> IsEquipmentDueSoonAsync(int equipmentId, int warningDays = 30);
    Task<List<string>> GetEquipmentTypesAsync();
}