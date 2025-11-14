# Migration System Developer Guide

## Overview

This guide provides comprehensive information for developers who need to extend, modify, or integrate with the Migration System. It covers architecture patterns, extension points, best practices, and common development scenarios.

## Getting Started

### Prerequisites

- **.NET 8.0 SDK** or later
- **Visual Studio 2022** or **VS Code** with C# extension
- **SQL Server** (LocalDB for development)
- **Node.js 18+** (for frontend development)
- **Angular CLI 17+**

### Development Environment Setup

1. **Clone the Repository**
   ```bash
   git clone https://github.com/your-org/lab-results-system.git
   cd lab-results-system
   ```

2. **Setup Database**
   ```bash
   # Start SQL Server LocalDB
   sqllocaldb start MSSQLLocalDB
   
   # Run database setup script
   ./scripts/setup-dev-database.sh
   ```

3. **Configure Development Settings**
   ```json
   // appsettings.Development.json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=LabResults_Dev;Trusted_Connection=true;",
       "LegacyConnection": "Server=(localdb)\\MSSQLLocalDB;Database=LabResults_Legacy;Trusted_Connection=true;"
     },
     "MigrationSettings": {
       "CsvDirectory": "./db-seeding/",
       "SqlDirectory": "./db-tables/",
       "BackupDirectory": "./Backups/"
     }
   }
   ```

4. **Install Dependencies**
   ```bash
   # Backend
   dotnet restore
   
   # Frontend
   cd lab-results-frontend
   npm install
   ```

5. **Run the Application**
   ```bash
   # Backend
   dotnet run --project LabResultsApi
   
   # Frontend (in separate terminal)
   cd lab-results-frontend
   ng serve
   ```

## Architecture Overview

### Core Patterns

The Migration System follows several key architectural patterns:

#### 1. Service Layer Pattern
All business logic is encapsulated in service classes that implement interfaces:

```csharp
public interface IMigrationControlService
{
    Task<MigrationResult> ExecuteFullMigrationAsync(MigrationOptions options);
    Task<MigrationStatus> GetMigrationStatusAsync(string migrationId);
}

[Service(ServiceLifetime.Scoped)]
public class MigrationControlService : IMigrationControlService
{
    private readonly IDatabaseSeedingService _seedingService;
    private readonly ISqlValidationService _validationService;
    private readonly ILogger<MigrationControlService> _logger;
    
    // Implementation...
}
```

#### 2. Repository Pattern
Data access is abstracted through repository interfaces:

```csharp
public interface IMigrationRepository
{
    Task<Migration> GetByIdAsync(string migrationId);
    Task<Migration> CreateAsync(Migration migration);
    Task UpdateAsync(Migration migration);
}
```

#### 3. Strategy Pattern
Different migration strategies can be implemented and selected at runtime:

```csharp
public interface IMigrationStrategy
{
    Task<MigrationResult> ExecuteAsync(MigrationContext context);
}

public class FullMigrationStrategy : IMigrationStrategy
{
    public async Task<MigrationResult> ExecuteAsync(MigrationContext context)
    {
        // Implementation for full migration
    }
}
```

#### 4. Observer Pattern
Progress and status updates are broadcast to observers:

```csharp
public interface IMigrationObserver
{
    Task OnProgressUpdated(MigrationProgress progress);
    Task OnStatusChanged(MigrationStatus status);
    Task OnCompleted(MigrationResult result);
}
```

## Extension Points

### 1. Custom Data Processors

Create custom processors for specific data transformation needs:

```csharp
public interface IDataProcessor
{
    string ProcessorName { get; }
    bool CanProcess(string tableName, DataRow row);
    Task<ProcessedData> ProcessAsync(string tableName, DataRow row);
}

[DataProcessor("CustomProcessor")]
public class CustomDataProcessor : IDataProcessor
{
    public string ProcessorName => "CustomProcessor";
    
    public bool CanProcess(string tableName, DataRow row)
    {
        return tableName == "SpecialTable";
    }
    
    public async Task<ProcessedData> ProcessAsync(string tableName, DataRow row)
    {
        // Custom processing logic
        return new ProcessedData
        {
            TransformedData = TransformData(row),
            ValidationResults = ValidateData(row)
        };
    }
}
```

### 2. Custom Validators

Implement custom validation logic for specific business rules:

```csharp
public interface ICustomValidator
{
    string ValidatorName { get; }
    Task<ValidationResult> ValidateAsync(object data, ValidationContext context);
}

[CustomValidator("BusinessRuleValidator")]
public class BusinessRuleValidator : ICustomValidator
{
    public string ValidatorName => "BusinessRuleValidator";
    
    public async Task<ValidationResult> ValidateAsync(object data, ValidationContext context)
    {
        var result = new ValidationResult();
        
        // Implement custom validation logic
        if (data is TestResult testResult)
        {
            if (testResult.Value < 0)
            {
                result.AddError("Test result cannot be negative");
            }
        }
        
        return result;
    }
}
```

### 3. Custom Notification Providers

Add support for additional notification channels:

```csharp
public interface INotificationProvider
{
    string ProviderName { get; }
    Task SendNotificationAsync(NotificationMessage message);
}

[NotificationProvider("Teams")]
public class TeamsNotificationProvider : INotificationProvider
{
    public string ProviderName => "Teams";
    
    public async Task SendNotificationAsync(NotificationMessage message)
    {
        // Implement Teams webhook integration
        var teamsMessage = new
        {
            text = message.Content,
            title = message.Subject,
            themeColor = GetColorForSeverity(message.Severity)
        };
        
        await _httpClient.PostAsJsonAsync(_webhookUrl, teamsMessage);
    }
}
```

### 4. Custom Migration Steps

Add custom steps to the migration process:

```csharp
public interface IMigrationStep
{
    string StepName { get; }
    int Order { get; }
    Task<StepResult> ExecuteAsync(MigrationContext context);
}

[MigrationStep("CustomDataTransformation", Order = 150)]
public class CustomDataTransformationStep : IMigrationStep
{
    public string StepName => "CustomDataTransformation";
    public int Order => 150; // Execute after seeding (100) but before validation (200)
    
    public async Task<StepResult> ExecuteAsync(MigrationContext context)
    {
        // Custom transformation logic
        return new StepResult
        {
            Success = true,
            Message = "Custom transformation completed"
        };
    }
}
```

## Service Registration

### Dependency Injection Setup

Register your custom services in `Program.cs`:

```csharp
// Core services
builder.Services.AddScoped<IMigrationControlService, MigrationControlService>();
builder.Services.AddScoped<IDatabaseSeedingService, DatabaseSeedingService>();

// Custom processors
builder.Services.AddScoped<IDataProcessor, CustomDataProcessor>();

// Custom validators
builder.Services.AddScoped<ICustomValidator, BusinessRuleValidator>();

// Custom notification providers
builder.Services.AddScoped<INotificationProvider, TeamsNotificationProvider>();

// Auto-register services with attributes
builder.Services.AddServicesWithAttribute<ServiceAttribute>();
builder.Services.AddProcessorsWithAttribute<DataProcessorAttribute>();
builder.Services.AddValidatorsWithAttribute<CustomValidatorAttribute>();
```

### Service Attributes

Use attributes to automatically register services:

```csharp
[AttributeUsage(AttributeTargets.Class)]
public class ServiceAttribute : Attribute
{
    public ServiceLifetime Lifetime { get; set; } = ServiceLifetime.Scoped;
}

[AttributeUsage(AttributeTargets.Class)]
public class DataProcessorAttribute : Attribute
{
    public string Name { get; set; }
    public DataProcessorAttribute(string name) => Name = name;
}
```

## Configuration Management

### Custom Configuration Sections

Add custom configuration sections for your extensions:

```csharp
public class CustomProcessorSettings
{
    public const string SectionName = "CustomProcessor";
    
    public bool EnableAdvancedFeatures { get; set; }
    public int BatchSize { get; set; } = 1000;
    public Dictionary<string, string> ProcessorOptions { get; set; } = new();
}

// Register in Program.cs
builder.Services.Configure<CustomProcessorSettings>(
    builder.Configuration.GetSection(CustomProcessorSettings.SectionName));
```

### Configuration Validation

Implement configuration validation:

```csharp
public class CustomProcessorSettingsValidator : IValidateOptions<CustomProcessorSettings>
{
    public ValidateOptionsResult Validate(string name, CustomProcessorSettings options)
    {
        var failures = new List<string>();
        
        if (options.BatchSize <= 0)
            failures.Add("BatchSize must be greater than 0");
            
        if (failures.Any())
            return ValidateOptionsResult.Fail(failures);
            
        return ValidateOptionsResult.Success;
    }
}

// Register validator
builder.Services.AddSingleton<IValidateOptions<CustomProcessorSettings>, 
    CustomProcessorSettingsValidator>();
```

## Database Integration

### Custom Entity Framework Models

Add custom models for your extensions:

```csharp
public class CustomMigrationLog
{
    public int Id { get; set; }
    public string MigrationId { get; set; }
    public string ProcessorName { get; set; }
    public string Operation { get; set; }
    public DateTime Timestamp { get; set; }
    public string Details { get; set; }
}

// Add to DbContext
public class LabDbContext : DbContext
{
    public DbSet<CustomMigrationLog> CustomMigrationLogs { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CustomMigrationLog>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.MigrationId).IsRequired().HasMaxLength(50);
            entity.Property(e => e.ProcessorName).IsRequired().HasMaxLength(100);
            entity.HasIndex(e => e.MigrationId);
        });
    }
}
```

### Custom Repositories

Implement custom repositories for your entities:

```csharp
public interface ICustomMigrationLogRepository
{
    Task<IEnumerable<CustomMigrationLog>> GetByMigrationIdAsync(string migrationId);
    Task AddAsync(CustomMigrationLog log);
}

[Service]
public class CustomMigrationLogRepository : ICustomMigrationLogRepository
{
    private readonly LabDbContext _context;
    
    public CustomMigrationLogRepository(LabDbContext context)
    {
        _context = context;
    }
    
    public async Task<IEnumerable<CustomMigrationLog>> GetByMigrationIdAsync(string migrationId)
    {
        return await _context.CustomMigrationLogs
            .Where(l => l.MigrationId == migrationId)
            .OrderBy(l => l.Timestamp)
            .ToListAsync();
    }
    
    public async Task AddAsync(CustomMigrationLog log)
    {
        _context.CustomMigrationLogs.Add(log);
        await _context.SaveChangesAsync();
    }
}
```

## Testing Extensions

### Unit Testing Custom Services

```csharp
public class CustomDataProcessorTests
{
    private readonly Mock<ILogger<CustomDataProcessor>> _loggerMock;
    private readonly CustomDataProcessor _processor;
    
    public CustomDataProcessorTests()
    {
        _loggerMock = new Mock<ILogger<CustomDataProcessor>>();
        _processor = new CustomDataProcessor(_loggerMock.Object);
    }
    
    [Fact]
    public async Task ProcessAsync_ValidData_ReturnsProcessedData()
    {
        // Arrange
        var dataRow = CreateTestDataRow();
        
        // Act
        var result = await _processor.ProcessAsync("SpecialTable", dataRow);
        
        // Assert
        result.Should().NotBeNull();
        result.TransformedData.Should().NotBeNull();
        result.ValidationResults.IsValid.Should().BeTrue();
    }
    
    [Fact]
    public void CanProcess_SpecialTable_ReturnsTrue()
    {
        // Arrange
        var dataRow = CreateTestDataRow();
        
        // Act
        var canProcess = _processor.CanProcess("SpecialTable", dataRow);
        
        // Assert
        canProcess.Should().BeTrue();
    }
}
```

### Integration Testing

```csharp
public class CustomProcessorIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    
    public CustomProcessorIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }
    
    [Fact]
    public async Task MigrationWithCustomProcessor_CompletesSuccessfully()
    {
        // Arrange
        var migrationRequest = new MigrationRequest
        {
            IncludeTables = new[] { "SpecialTable" },
            UseCustomProcessors = true
        };
        
        // Act
        var response = await _client.PostAsJsonAsync("/api/Migration/start", migrationRequest);
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var result = await response.Content.ReadFromJsonAsync<MigrationResponse>();
        result.Success.Should().BeTrue();
    }
}
```

## Logging and Monitoring

### Custom Logging

Implement structured logging for your extensions:

```csharp
public static class LoggerExtensions
{
    private static readonly Action<ILogger, string, string, Exception> _customProcessorStarted =
        LoggerMessage.Define<string, string>(
            LogLevel.Information,
            new EventId(1001, "CustomProcessorStarted"),
            "Custom processor {ProcessorName} started for table {TableName}");
    
    public static void CustomProcessorStarted(this ILogger logger, string processorName, string tableName)
    {
        _customProcessorStarted(logger, processorName, tableName, null);
    }
}

// Usage in your processor
public class CustomDataProcessor : IDataProcessor
{
    private readonly ILogger<CustomDataProcessor> _logger;
    
    public async Task<ProcessedData> ProcessAsync(string tableName, DataRow row)
    {
        _logger.CustomProcessorStarted(ProcessorName, tableName);
        
        // Processing logic...
    }
}
```

### Custom Metrics

Add custom metrics for monitoring:

```csharp
public class CustomProcessorMetrics
{
    private readonly IMetricsLogger _metricsLogger;
    private readonly Counter<int> _processedRecords;
    private readonly Histogram<double> _processingDuration;
    
    public CustomProcessorMetrics(IMetricsLogger metricsLogger)
    {
        _metricsLogger = metricsLogger;
        _processedRecords = metricsLogger.CreateCounter<int>("custom_processor_records_processed");
        _processingDuration = metricsLogger.CreateHistogram<double>("custom_processor_duration_seconds");
    }
    
    public void RecordProcessedRecord(string tableName)
    {
        _processedRecords.Add(1, new KeyValuePair<string, object>("table", tableName));
    }
    
    public void RecordProcessingDuration(double durationSeconds, string tableName)
    {
        _processingDuration.Record(durationSeconds, new KeyValuePair<string, object>("table", tableName));
    }
}
```

## Error Handling

### Custom Exception Types

Define custom exceptions for your extensions:

```csharp
public class CustomProcessorException : Exception
{
    public string ProcessorName { get; }
    public string TableName { get; }
    
    public CustomProcessorException(string processorName, string tableName, string message)
        : base(message)
    {
        ProcessorName = processorName;
        TableName = tableName;
    }
    
    public CustomProcessorException(string processorName, string tableName, string message, Exception innerException)
        : base(message, innerException)
    {
        ProcessorName = processorName;
        TableName = tableName;
    }
}
```

### Global Exception Handling

Register custom exception handlers:

```csharp
public class CustomProcessorExceptionHandler : IExceptionHandler
{
    private readonly ILogger<CustomProcessorExceptionHandler> _logger;
    
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is CustomProcessorException customEx)
        {
            _logger.LogError(customEx, 
                "Custom processor {ProcessorName} failed for table {TableName}",
                customEx.ProcessorName, customEx.TableName);
            
            var response = new ErrorResponse
            {
                Error = "CustomProcessorError",
                Message = customEx.Message,
                Details = new
                {
                    ProcessorName = customEx.ProcessorName,
                    TableName = customEx.TableName
                }
            };
            
            httpContext.Response.StatusCode = 422;
            await httpContext.Response.WriteAsJsonAsync(response, cancellationToken);
            return true;
        }
        
        return false;
    }
}
```

## Performance Optimization

### Async Patterns

Use proper async patterns in your extensions:

```csharp
public class OptimizedDataProcessor : IDataProcessor
{
    public async Task<ProcessedData> ProcessAsync(string tableName, DataRow row)
    {
        // Use ConfigureAwait(false) for library code
        var validationTask = ValidateDataAsync(row).ConfigureAwait(false);
        var transformationTask = TransformDataAsync(row).ConfigureAwait(false);
        
        // Process in parallel when possible
        await Task.WhenAll(validationTask.AsTask(), transformationTask.AsTask());
        
        return new ProcessedData
        {
            ValidationResults = await validationTask,
            TransformedData = await transformationTask
        };
    }
}
```

### Memory Management

Implement proper memory management:

```csharp
public class MemoryEfficientProcessor : IDataProcessor, IDisposable
{
    private readonly MemoryPool<byte> _memoryPool;
    private bool _disposed;
    
    public MemoryEfficientProcessor()
    {
        _memoryPool = MemoryPool<byte>.Shared;
    }
    
    public async Task<ProcessedData> ProcessAsync(string tableName, DataRow row)
    {
        using var memory = _memoryPool.Rent(1024);
        
        // Use rented memory for processing
        var buffer = memory.Memory.Span;
        
        // Processing logic...
        
        return result;
    }
    
    public void Dispose()
    {
        if (!_disposed)
        {
            _memoryPool?.Dispose();
            _disposed = true;
        }
    }
}
```

## Frontend Extensions

### Custom Angular Components

Create custom components for migration UI:

```typescript
@Component({
  selector: 'app-custom-processor-config',
  template: `
    <mat-card>
      <mat-card-header>
        <mat-card-title>Custom Processor Configuration</mat-card-title>
      </mat-card-header>
      <mat-card-content>
        <form [formGroup]="configForm" (ngSubmit)="onSubmit()">
          <mat-form-field>
            <mat-label>Batch Size</mat-label>
            <input matInput type="number" formControlName="batchSize">
          </mat-form-field>
          
          <mat-checkbox formControlName="enableAdvancedFeatures">
            Enable Advanced Features
          </mat-checkbox>
          
          <button mat-raised-button color="primary" type="submit">
            Save Configuration
          </button>
        </form>
      </mat-card-content>
    </mat-card>
  `
})
export class CustomProcessorConfigComponent {
  configForm = this.fb.group({
    batchSize: [1000, [Validators.required, Validators.min(1)]],
    enableAdvancedFeatures: [false]
  });
  
  constructor(private fb: FormBuilder, private configService: ConfigService) {}
  
  onSubmit() {
    if (this.configForm.valid) {
      this.configService.updateCustomProcessorConfig(this.configForm.value)
        .subscribe(result => {
          // Handle success
        });
    }
  }
}
```

### Custom Services

Create Angular services for your extensions:

```typescript
@Injectable({
  providedIn: 'root'
})
export class CustomProcessorService {
  private readonly apiUrl = '/api/CustomProcessor';
  
  constructor(private http: HttpClient) {}
  
  getProcessorStatus(processorName: string): Observable<ProcessorStatus> {
    return this.http.get<ProcessorStatus>(`${this.apiUrl}/${processorName}/status`);
  }
  
  configureProcessor(config: ProcessorConfig): Observable<ConfigResult> {
    return this.http.post<ConfigResult>(`${this.apiUrl}/configure`, config);
  }
  
  getProcessorMetrics(processorName: string, timeRange: TimeRange): Observable<ProcessorMetrics> {
    const params = new HttpParams()
      .set('from', timeRange.from.toISOString())
      .set('to', timeRange.to.toISOString());
      
    return this.http.get<ProcessorMetrics>(`${this.apiUrl}/${processorName}/metrics`, { params });
  }
}
```

## Deployment Considerations

### Docker Support

Add Docker support for your extensions:

```dockerfile
# Add custom dependencies
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Install custom tools if needed
RUN apt-get update && apt-get install -y \
    custom-tool \
    && rm -rf /var/lib/apt/lists/*

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["LabResultsApi/LabResultsApi.csproj", "LabResultsApi/"]
COPY ["CustomExtensions/CustomExtensions.csproj", "CustomExtensions/"]
RUN dotnet restore "LabResultsApi/LabResultsApi.csproj"

COPY . .
WORKDIR "/src/LabResultsApi"
RUN dotnet build "LabResultsApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "LabResultsApi.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "LabResultsApi.dll"]
```

### Configuration Management

Use environment-specific configuration:

```json
{
  "CustomProcessor": {
    "EnableAdvancedFeatures": true,
    "BatchSize": "${CUSTOM_PROCESSOR_BATCH_SIZE:1000}",
    "ProcessorOptions": {
      "Option1": "${CUSTOM_OPTION_1:default_value}",
      "Option2": "${CUSTOM_OPTION_2:default_value}"
    }
  }
}
```

## Best Practices

### 1. Follow SOLID Principles

- **Single Responsibility**: Each class should have one reason to change
- **Open/Closed**: Open for extension, closed for modification
- **Liskov Substitution**: Derived classes must be substitutable for base classes
- **Interface Segregation**: Clients shouldn't depend on interfaces they don't use
- **Dependency Inversion**: Depend on abstractions, not concretions

### 2. Use Dependency Injection

Always use constructor injection for dependencies:

```csharp
public class CustomService
{
    private readonly IDependency1 _dependency1;
    private readonly IDependency2 _dependency2;
    
    public CustomService(IDependency1 dependency1, IDependency2 dependency2)
    {
        _dependency1 = dependency1 ?? throw new ArgumentNullException(nameof(dependency1));
        _dependency2 = dependency2 ?? throw new ArgumentNullException(nameof(dependency2));
    }
}
```

### 3. Implement Proper Error Handling

Use specific exception types and provide meaningful error messages:

```csharp
public async Task ProcessDataAsync(string tableName)
{
    try
    {
        await ProcessInternalAsync(tableName);
    }
    catch (SqlException ex) when (ex.Number == 2) // Timeout
    {
        throw new ProcessingTimeoutException($"Processing timeout for table {tableName}", ex);
    }
    catch (SqlException ex) when (ex.Number == 18456) // Login failed
    {
        throw new DatabaseConnectionException("Database authentication failed", ex);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Unexpected error processing table {TableName}", tableName);
        throw new ProcessingException($"Failed to process table {tableName}", ex);
    }
}
```

### 4. Use Async/Await Properly

Follow async best practices:

```csharp
// Good
public async Task<Result> ProcessAsync()
{
    var data = await GetDataAsync().ConfigureAwait(false);
    return await ProcessDataAsync(data).ConfigureAwait(false);
}

// Avoid
public async Task<Result> ProcessAsync()
{
    var data = GetDataAsync().Result; // Blocking call
    return ProcessDataAsync(data).Result; // Blocking call
}
```

### 5. Implement Comprehensive Logging

Use structured logging with correlation IDs:

```csharp
public async Task ProcessAsync(string correlationId)
{
    using var scope = _logger.BeginScope(new Dictionary<string, object>
    {
        ["CorrelationId"] = correlationId,
        ["Operation"] = "ProcessData"
    });
    
    _logger.LogInformation("Starting data processing");
    
    try
    {
        await ProcessInternalAsync();
        _logger.LogInformation("Data processing completed successfully");
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Data processing failed");
        throw;
    }
}
```

## Troubleshooting

### Common Issues

1. **Service Registration Issues**
   - Ensure all dependencies are registered
   - Check service lifetimes (Singleton, Scoped, Transient)
   - Verify interface implementations

2. **Configuration Issues**
   - Validate configuration sections exist
   - Check environment variable names
   - Ensure configuration binding is correct

3. **Database Issues**
   - Verify connection strings
   - Check database permissions
   - Ensure migrations are applied

4. **Performance Issues**
   - Profile memory usage
   - Check for blocking operations
   - Monitor database query performance

### Debugging Tips

1. **Enable Detailed Logging**
   ```json
   {
     "Logging": {
       "LogLevel": {
         "Default": "Information",
         "YourNamespace": "Debug"
       }
     }
   }
   ```

2. **Use Application Insights**
   ```csharp
   builder.Services.AddApplicationInsightsTelemetry();
   ```

3. **Add Health Checks**
   ```csharp
   builder.Services.AddHealthChecks()
       .AddDbContext<LabDbContext>()
       .AddCustomHealthCheck<CustomProcessorHealthCheck>();
   ```

## Contributing

### Code Style

Follow the established code style:
- Use PascalCase for public members
- Use camelCase for private fields
- Use meaningful names
- Add XML documentation for public APIs

### Pull Request Process

1. Create feature branch from main
2. Implement changes with tests
3. Update documentation
4. Submit pull request with description
5. Address review feedback
6. Merge after approval

### Testing Requirements

- Unit tests for all business logic
- Integration tests for database operations
- End-to-end tests for critical paths
- Minimum 80% code coverage

## Resources

### Documentation
- [ASP.NET Core Documentation](https://docs.microsoft.com/en-us/aspnet/core/)
- [Entity Framework Core Documentation](https://docs.microsoft.com/en-us/ef/core/)
- [Angular Documentation](https://angular.io/docs)

### Tools
- [Visual Studio](https://visualstudio.microsoft.com/)
- [VS Code](https://code.visualstudio.com/)
- [SQL Server Management Studio](https://docs.microsoft.com/en-us/sql/ssms/)
- [Postman](https://www.postman.com/) for API testing

### Community
- [Stack Overflow](https://stackoverflow.com/)
- [GitHub Discussions](https://github.com/your-org/lab-results-system/discussions)
- [Developer Slack Channel](https://your-org.slack.com/channels/developers)