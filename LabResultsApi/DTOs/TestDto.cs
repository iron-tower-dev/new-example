namespace LabResultsApi.DTOs;

public class TestDto
{
    public int TestId { get; set; }
    public string TestName { get; set; } = string.Empty;
    public string? TestDescription { get; set; }
    public bool Active { get; set; }
}

public class TestTemplateDto
{
    public int TestId { get; set; }
    public string TestName { get; set; } = string.Empty;
    public string? TestDescription { get; set; }
    public List<TestFieldDto> Fields { get; set; } = new();
    public int MaxTrials { get; set; } = 4;
    public bool RequiresCalculation { get; set; }
    public bool SupportsFileUpload { get; set; }
    public string? CalculationFormula { get; set; }
    public Dictionary<string, object>? ValidationRules { get; set; }
}

public class TestFieldDto
{
    public string FieldName { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string FieldType { get; set; } = string.Empty; // "number", "text", "dropdown", "file"
    public bool IsRequired { get; set; }
    public bool IsCalculated { get; set; }
    public string? ValidationPattern { get; set; }
    public double? MinValue { get; set; }
    public double? MaxValue { get; set; }
    public int? DecimalPlaces { get; set; }
    public List<string>? DropdownOptions { get; set; }
    public string? Unit { get; set; }
    public string? HelpText { get; set; }
}

public class TestResultDto
{
    public int SampleId { get; set; }
    public int TestId { get; set; }
    public List<TrialResultDto> Trials { get; set; } = new();
    public string? Comments { get; set; }
    public string Status { get; set; } = "X"; // X = Pending, E = In Progress, C = Complete
    public string? EntryId { get; set; }
    public DateTime? EntryDate { get; set; }
}

public class TrialResultDto
{
    public int TrialNumber { get; set; }
    public Dictionary<string, object?> Values { get; set; } = new();
    public double? CalculatedResult { get; set; }
    public bool IsComplete { get; set; }
}

public class SaveTestResultRequest
{
    public int SampleId { get; set; }
    public int TestId { get; set; }
    public List<TrialResultDto> Trials { get; set; } = new();
    public string? Comments { get; set; }
    public string EntryId { get; set; } = string.Empty;
}

public class TestCalculationRequest
{
    public int TestId { get; set; }
    public Dictionary<string, object?> InputValues { get; set; } = new();
}

public class TestCalculationResult
{
    public double? Result { get; set; }
    public bool IsValid { get; set; }
    public string? ErrorMessage { get; set; }
    public Dictionary<string, object>? IntermediateValues { get; set; }
}