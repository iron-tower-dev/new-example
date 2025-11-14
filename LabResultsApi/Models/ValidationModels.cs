using System.ComponentModel.DataAnnotations;

namespace LabResultsApi.Models;

public class ValidationException : Exception
{
    public Dictionary<string, List<string>> Errors { get; }

    public ValidationException(string message) : base(message)
    {
        Errors = new Dictionary<string, List<string>>();
    }

    public ValidationException(string message, Dictionary<string, List<string>> errors) : base(message)
    {
        Errors = errors;
    }

    public ValidationException(string field, string error) : base($"Validation failed for {field}")
    {
        Errors = new Dictionary<string, List<string>>
        {
            { field, new List<string> { error } }
        };
    }

    public void AddError(string field, string error)
    {
        if (!Errors.ContainsKey(field))
        {
            Errors[field] = new List<string>();
        }
        Errors[field].Add(error);
    }
}

public class ValidationResult
{
    public bool IsValid { get; set; }
    public Dictionary<string, List<string>> Errors { get; set; } = new();
    public Dictionary<string, List<string>> Warnings { get; set; } = new();

    public void AddError(string field, string message)
    {
        if (!Errors.ContainsKey(field))
        {
            Errors[field] = new List<string>();
        }
        Errors[field].Add(message);
        IsValid = false;
    }

    public void AddWarning(string field, string message)
    {
        if (!Warnings.ContainsKey(field))
        {
            Warnings[field] = new List<string>();
        }
        Warnings[field].Add(message);
    }

    public bool HasErrors => Errors.Any(e => e.Value.Any());
    public bool HasWarnings => Warnings.Any(w => w.Value.Any());
}

public class TestValidationRules
{
    public int TestId { get; set; }
    public string TestName { get; set; } = string.Empty;
    public Dictionary<string, List<FieldValidationRule>> FieldRules { get; set; } = new();
    public List<CrossFieldValidationRule> CrossFieldRules { get; set; } = new();
}

public class FieldValidationRule
{
    public string Type { get; set; } = string.Empty; // required, numeric, positive, range, etc.
    public Dictionary<string, object>? Parameters { get; set; }
    public string? Message { get; set; }
    public string Severity { get; set; } = "error"; // error, warning, info
}

public class CrossFieldValidationRule
{
    public List<string> Fields { get; set; } = new();
    public string ValidationType { get; set; } = string.Empty;
    public Dictionary<string, object>? Parameters { get; set; }
    public string Message { get; set; } = string.Empty;
    public string Severity { get; set; } = "error";
}

// Validation attributes for test-specific validation
public class TanValueAttribute : ValidationAttribute
{
    public override bool IsValid(object? value)
    {
        if (value == null) return true; // Let Required handle null values

        if (double.TryParse(value.ToString(), out double tanValue))
        {
            if (tanValue < 0.01)
            {
                ErrorMessage = "TAN value cannot be less than 0.01 mg KOH/g";
                return false;
            }
            if (tanValue > 50)
            {
                ErrorMessage = "TAN value seems unusually high (>50 mg KOH/g). Please verify.";
                return false;
            }
            return true;
        }

        ErrorMessage = "TAN value must be a valid number";
        return false;
    }
}

public class ViscosityValueAttribute : ValidationAttribute
{
    public string TestType { get; set; } = "40C";

    public override bool IsValid(object? value)
    {
        if (value == null) return true;

        if (double.TryParse(value.ToString(), out double viscValue))
        {
            if (viscValue <= 0)
            {
                ErrorMessage = "Viscosity must be greater than zero";
                return false;
            }

            var (min, max, unit) = TestType switch
            {
                "40C" => (1.0, 10000.0, "cSt @ 40°C"),
                "100C" => (1.0, 1000.0, "cSt @ 100°C"),
                _ => (1.0, 10000.0, "cSt")
            };

            if (viscValue < min || viscValue > max)
            {
                ErrorMessage = $"{unit} should be between {min} and {max}";
                return false;
            }
            return true;
        }

        ErrorMessage = "Viscosity value must be a valid number";
        return false;
    }
}

public class SampleWeightAttribute : ValidationAttribute
{
    public override bool IsValid(object? value)
    {
        if (value == null) return true;

        if (double.TryParse(value.ToString(), out double weight))
        {
            if (weight <= 0)
            {
                ErrorMessage = "Sample weight must be greater than zero";
                return false;
            }
            if (weight < 0.1)
            {
                ErrorMessage = "Sample weight is very low (<0.1g). Results may be inaccurate.";
                return false;
            }
            if (weight > 100)
            {
                ErrorMessage = "Sample weight is very high (>100g). Please verify.";
                return false;
            }
            return true;
        }

        ErrorMessage = "Sample weight must be a valid number";
        return false;
    }
}

public class ParticleCountAttribute : ValidationAttribute
{
    public override bool IsValid(object? value)
    {
        if (value == null) return true;

        if (double.TryParse(value.ToString(), out double count))
        {
            if (count < 0)
            {
                ErrorMessage = "Particle count cannot be negative";
                return false;
            }
            if (count > 1000000)
            {
                ErrorMessage = "Particle count seems unusually high (>1,000,000). Please verify.";
                return false;
            }
            return true;
        }

        ErrorMessage = "Particle count must be a valid number";
        return false;
    }
}

public class FlashPointAttribute : ValidationAttribute
{
    public override bool IsValid(object? value)
    {
        if (value == null) return true;

        if (double.TryParse(value.ToString(), out double flashPoint))
        {
            if (flashPoint < -50 || flashPoint > 800)
            {
                ErrorMessage = "Flash point should be between -50°F and 800°F";
                return false;
            }
            return true;
        }

        ErrorMessage = "Flash point must be a valid number";
        return false;
    }
}

// DTO validation models
public class TanTrialValidationDto
{
    [Required(ErrorMessage = "Sample weight is required")]
    [SampleWeight]
    public double? SampleWeight { get; set; }

    [Required(ErrorMessage = "Final buret reading is required")]
    [Range(0, double.MaxValue, ErrorMessage = "Final buret reading must be 0 or greater")]
    public double? FinalBuret { get; set; }

    [TanValue]
    public double? TanResult { get; set; }
}

public class ViscosityTrialValidationDto
{
    [Required(ErrorMessage = "Stopwatch time is required")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Stopwatch time must be greater than 0")]
    public double? StopwatchTime { get; set; }

    [Required(ErrorMessage = "Tube ID is required")]
    public int? TubeId { get; set; }

    [ViscosityValue]
    public double? ViscosityResult { get; set; }
}

public class ParticleCountTrialValidationDto
{
    [ParticleCount]
    public double? Count5to10 { get; set; }

    [ParticleCount]
    public double? Count10to15 { get; set; }

    [ParticleCount]
    public double? Count15to25 { get; set; }

    [ParticleCount]
    public double? Count25to50 { get; set; }

    [ParticleCount]
    public double? Count50to100 { get; set; }

    [ParticleCount]
    public double? CountOver100 { get; set; }

    public int? NasLookupResult { get; set; }
}

public class FlashPointTrialValidationDto
{
    [Required(ErrorMessage = "Flash point temperature is required")]
    [FlashPoint]
    public double? FlashPointTemp { get; set; }

    [Required(ErrorMessage = "Barometric pressure is required")]
    [Range(500, 900, ErrorMessage = "Barometric pressure should be between 500 and 900 mmHg")]
    public double? BarometricPressure { get; set; }

    public double? CorrectedFlashPoint { get; set; }
}public 
class CalculationResult
{
    public double? Result { get; set; }
    public bool IsValid { get; set; }
    public string? ErrorMessage { get; set; }
    public Dictionary<string, object>? IntermediateValues { get; set; }
}

public class NasCalculationResult
{
    public int HighestNas { get; set; }
    public Dictionary<int, int> ChannelNasValues { get; set; } = new();
    public bool IsValid { get; set; }
    public string? ErrorMessage { get; set; }
}

public class NlgiLookupResult
{
    public double PenetrationValue { get; set; }
    public string? NlgiGrade { get; set; }
    public bool IsValid { get; set; }
    public string? ErrorMessage { get; set; }
}