namespace LabResultsApi.Models.Migration;

public class ErrorRecoveryStrategy
{
    public ErrorLevel Level { get; set; }
    public RecoveryAction Action { get; set; }
    public int MaxRetries { get; set; } = 3;
    public TimeSpan RetryDelay { get; set; } = TimeSpan.FromSeconds(5);
    public bool ContinueOnFailure { get; set; } = true;
    public string? CustomMessage { get; set; }
}

public enum RecoveryAction
{
    Retry,
    Skip,
    Abort,
    Prompt,
    UseDefault
}

public class RetryPolicy
{
    public int MaxRetries { get; set; } = 3;
    public TimeSpan InitialDelay { get; set; } = TimeSpan.FromSeconds(1);
    public TimeSpan MaxDelay { get; set; } = TimeSpan.FromMinutes(1);
    public double BackoffMultiplier { get; set; } = 2.0;
    public bool UseExponentialBackoff { get; set; } = true;
    
    public TimeSpan GetDelay(int attemptNumber)
    {
        if (!UseExponentialBackoff)
            return InitialDelay;
            
        var delay = TimeSpan.FromMilliseconds(
            InitialDelay.TotalMilliseconds * Math.Pow(BackoffMultiplier, attemptNumber - 1));
            
        return delay > MaxDelay ? MaxDelay : delay;
    }
}

public class ErrorContext
{
    public string Component { get; set; } = string.Empty;
    public string Operation { get; set; } = string.Empty;
    public string? TableName { get; set; }
    public int? RecordNumber { get; set; }
    public Dictionary<string, object> AdditionalData { get; set; } = new();
    public Exception? Exception { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}