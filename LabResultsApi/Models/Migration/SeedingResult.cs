namespace LabResultsApi.Models.Migration;

public class SeedingResult
{
    public int TablesProcessed { get; set; }
    public int TablesCreated { get; set; }
    public int RecordsInserted { get; set; }
    public int RecordsSkipped { get; set; }
    public TimeSpan Duration { get; set; }
    public List<TableSeedingResult> TableResults { get; set; } = new();
    public bool Success => TableResults.All(t => t.Success);
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
}

public class TableSeedingResult
{
    public string TableName { get; set; } = string.Empty;
    public bool Success { get; set; }
    public int RecordsProcessed { get; set; }
    public int RecordsInserted { get; set; }
    public int RecordsSkipped { get; set; }
    public List<string> Errors { get; set; } = new();
    public TimeSpan Duration { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string? CsvFilePath { get; set; }
    public bool TableCreated { get; set; }
}

public class TableCreationResult
{
    public int TablesCreated { get; set; }
    public int TablesSkipped { get; set; }
    public List<TableCreationDetail> CreationDetails { get; set; } = new();
    public bool Success => CreationDetails.All(t => t.Success);
    public TimeSpan Duration { get; set; }
}

public class TableCreationDetail
{
    public string TableName { get; set; } = string.Empty;
    public bool Success { get; set; }
    public string? Error { get; set; }
    public string? SqlFilePath { get; set; }
    public TimeSpan Duration { get; set; }
    public bool AlreadyExists { get; set; }
}