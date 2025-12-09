using LabResultsApi.Data;
using LabResultsApi.DTOs;
using LabResultsApi.Models;
using Microsoft.EntityFrameworkCore;

namespace LabResultsApi.Services;

public class EquipmentService : IEquipmentService
{
    private readonly LabDbContext _context;
    private readonly ILogger<EquipmentService> _logger;

    public EquipmentService(LabDbContext context, ILogger<EquipmentService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<EquipmentSelectionDto>> GetEquipmentByTypeAsync(string equipType, short? testId = null, string? lubeType = null)
    {
        try
        {
            // Enhanced M&TE logic matching legacy system
            var query = _context.Equipment
                .Where(e => e.EquipType == equipType && (e.Exclude == null || e.Exclude != true));

            // Filter by test ID if provided (legacy M&TE logic)
            if (testId.HasValue)
            {
                query = query.Where(e => e.TestId == null || e.TestId == testId.Value);
            }

            // Note: LubeType filtering removed as Equipment table doesn't have LubeType column
            // LubeType is associated with samples, not equipment

            var equipment = await query
                .OrderBy(e => e.EquipName)
                .ToListAsync();

            var result = new List<EquipmentSelectionDto>();

            foreach (var item in equipment)
            {
                var validation = await ValidateEquipmentAsync(item.Id);
                
                var dto = new EquipmentSelectionDto
                {
                    Id = item.Id,
                    EquipName = item.EquipName ?? string.Empty,
                    EquipType = item.EquipType,
                    DueDate = item.DueDate,
                    CalibrationValue = GetCalibrationValueForType(item, equipType),
                    IsOverdue = validation.IsOverdue,
                    IsDueSoon = validation.IsDueSoon,
                    IsValid = validation.IsValid,
                    ValidationMessage = validation.Message
                };

                // Enhanced display text with M&TE status indicators (matching legacy system)
                var statusIndicator = "";
                if (dto.IsOverdue)
                {
                    statusIndicator = "**"; // Overdue indicator
                }
                else if (dto.IsDueSoon)
                {
                    statusIndicator = "*"; // Due soon indicator
                }

                var dueDateText = item.DueDate.HasValue ? 
                    $"({item.DueDate.Value:MM/dd/yyyy}{statusIndicator})" : "(no date)";
                
                dto.DisplayText = $"{dto.EquipName} {dueDateText}";

                // Only include valid equipment (not overdue) unless specifically requested
                if (validation.IsValid || validation.IsDueSoon)
                {
                    result.Add(dto);
                }
                else
                {
                    // Add overdue equipment but mark as disabled
                    dto.IsDisabled = true;
                    result.Add(dto);
                }
            }

            // Sort by validation status first (valid equipment first), then by name
            return result.OrderBy(e => e.IsOverdue ? 1 : 0)
                        .ThenBy(e => e.IsDueSoon ? 1 : 0)
                        .ThenBy(e => e.EquipName)
                        .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving equipment by type {EquipType}", equipType);
            throw;
        }
    }

    public async Task<EquipmentCalibrationDto?> GetEquipmentCalibrationAsync(int equipmentId)
    {
        try
        {
            var equipment = await _context.Equipment
                .FirstOrDefaultAsync(e => e.Id == equipmentId);

            if (equipment == null)
                return null;

            var validation = await ValidateEquipmentAsync(equipmentId);

            return new EquipmentCalibrationDto
            {
                EquipmentId = equipment.Id,
                EquipName = equipment.EquipName ?? string.Empty,
                EquipType = equipment.EquipType,
                CalibrationValue = GetCalibrationValueForType(equipment, equipment.EquipType),
                DueDate = equipment.DueDate,
                IsValid = validation.IsValid,
                ValidationMessage = validation.Message
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving calibration for equipment {EquipmentId}", equipmentId);
            throw;
        }
    }

    public async Task<EquipmentValidationResult> ValidateEquipmentAsync(int equipmentId)
    {
        try
        {
            var equipment = await _context.Equipment
                .FirstOrDefaultAsync(e => e.Id == equipmentId);

            if (equipment == null)
            {
                return new EquipmentValidationResult
                {
                    IsValid = false,
                    Message = "Equipment not found"
                };
            }

            if (equipment.Exclude == true)
            {
                return new EquipmentValidationResult
                {
                    IsValid = false,
                    Message = "Equipment is excluded from use"
                };
            }

            var result = new EquipmentValidationResult
            {
                DueDate = equipment.DueDate
            };

            if (equipment.DueDate.HasValue)
            {
                var daysUntilDue = (equipment.DueDate.Value - DateTime.Now).Days;
                result.DaysUntilDue = daysUntilDue;
                result.IsOverdue = daysUntilDue < 0;
                result.IsDueSoon = daysUntilDue >= 0 && daysUntilDue < 30;

                if (result.IsOverdue)
                {
                    result.IsValid = false;
                    result.Message = $"Equipment is overdue for calibration (due: {equipment.DueDate.Value:MM/dd/yyyy})";
                }
                else if (result.IsDueSoon)
                {
                    result.IsValid = true;
                    result.Message = $"Equipment calibration due soon ({equipment.DueDate.Value:MM/dd/yyyy})";
                }
                else
                {
                    result.IsValid = true;
                    result.Message = $"Equipment calibration valid until {equipment.DueDate.Value:MM/dd/yyyy}";
                }
            }
            else
            {
                result.IsValid = true;
                result.Message = "No calibration due date specified";
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating equipment {EquipmentId}", equipmentId);
            throw;
        }
    }

    public async Task<List<EquipmentDto>> GetAllEquipmentAsync(EquipmentFilterDto? filter = null)
    {
        try
        {
            var query = _context.Equipment.AsQueryable();

            if (filter != null)
            {
                if (!string.IsNullOrEmpty(filter.EquipType))
                {
                    query = query.Where(e => e.EquipType == filter.EquipType);
                }

                if (filter.TestId.HasValue)
                {
                    query = query.Where(e => e.TestId == null || e.TestId == filter.TestId.Value);
                }

                if (filter.ExcludeInactive)
                {
                    query = query.Where(e => e.Exclude == null || e.Exclude != true);
                }

                if (!filter.IncludeOverdue)
                {
                    query = query.Where(e => e.DueDate == null || e.DueDate >= DateTime.Now);
                }
            }

            var equipment = await query
                .OrderBy(e => e.EquipType)
                .ThenBy(e => e.EquipName)
                .ToListAsync();

            return EquipmentDto.ToDtoList(equipment);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all equipment");
            throw;
        }
    }

    public async Task<EquipmentDto?> GetEquipmentByIdAsync(int id)
    {
        try
        {
            var equipment = await _context.Equipment
                .FirstOrDefaultAsync(e => e.Id == id);

            return equipment != null ? EquipmentDto.ToDto(equipment) : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving equipment {EquipmentId}", id);
            throw;
        }
    }

    public async Task<bool> IsEquipmentDueSoonAsync(int equipmentId, int warningDays = 30)
    {
        try
        {
            var equipment = await _context.Equipment
                .FirstOrDefaultAsync(e => e.Id == equipmentId);

            if (equipment?.DueDate == null)
                return false;

            var daysUntilDue = (equipment.DueDate.Value - DateTime.Now).Days;
            return daysUntilDue >= 0 && daysUntilDue < warningDays;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking due date for equipment {EquipmentId}", equipmentId);
            throw;
        }
    }

    public async Task<List<string>> GetEquipmentTypesAsync()
    {
        try
        {
            return await _context.Equipment
                .Where(e => e.Exclude == null || e.Exclude != true)
                .Select(e => e.EquipType)
                .Distinct()
                .OrderBy(t => t)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving equipment types");
            throw;
        }
    }

    private static double? GetCalibrationValueForType(Equipment equipment, string equipType)
    {
        // Based on the legacy code, different equipment types use different calibration values
        return equipType.ToUpper() switch
        {
            "VISCOMETER" => equipment.Val1, // Tube calibration value
            "THERMOMETER" => equipment.Val1, // Temperature correction
            "BAROMETER" => equipment.Val1, // Pressure correction
            "TIMER" => equipment.Val1, // Timer calibration
            "DELETERIOUS" => equipment.Val1, // Deleterious test calibration
            _ => equipment.Val1 // Default to Val1
        };
    }
}