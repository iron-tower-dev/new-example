namespace LabResultsApi.Configuration;

public class PerformanceConfiguration
{
    public CacheSettings CacheSettings { get; set; } = new();
    public DatabaseSettings DatabaseSettings { get; set; } = new();
    public MonitoringSettings Monitoring { get; set; } = new();
}

public class CacheSettings
{
    public int DefaultExpirationMinutes { get; set; } = 5;
    public int LookupCacheExpirationHours { get; set; } = 1;
    public int MaxCacheSize { get; set; } = 1000;
}

public class DatabaseSettings
{
    public int CommandTimeoutSeconds { get; set; } = 30;
    public int ConnectionPoolSize { get; set; } = 128;
    public bool EnableRetryOnFailure { get; set; } = true;
    public int MaxRetryCount { get; set; } = 3;
    public int MaxRetryDelaySeconds { get; set; } = 5;
}

public class MonitoringSettings
{
    public int SlowQueryThresholdMs { get; set; } = 1000;
    public int SlowEndpointThresholdMs { get; set; } = 2000;
    public bool EnableDetailedLogging { get; set; } = false;
}