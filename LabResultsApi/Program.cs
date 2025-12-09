using LabResultsApi.Endpoints;
using LabResultsApi.Services;
using LabResultsApi.Data;
using LabResultsApi.Middleware;
using LabResultsApi.Configuration;
using LabResultsApi.Models;
using Microsoft.EntityFrameworkCore;
// JWT authentication imports removed for SSO migration
using NLog;
using NLog.Extensions.Logging;

var builder = WebApplication.CreateBuilder(args);

// Configure performance settings
builder.Services.Configure<PerformanceConfiguration>(
    builder.Configuration.GetSection("Performance"));

// Configure NLog
builder.Logging.ClearProviders();
builder.Logging.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
builder.Logging.AddNLog();

// Add services to the container
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Laboratory Test Results API", Version = "v1" });
});

// Add CORS
builder.Services.AddCors(options =>
{
    if (builder.Environment.IsDevelopment())
    {
        options.AddPolicy("AllowAngularApp", policy =>
        {
            policy.WithOrigins(
                    "http://localhost:4200",
                    "http://localhost:5173",  // Vite dev server
                    "http://localhost:38205"  // Alternative port
                  )
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        });
    }
    else
    {
        var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() 
            ?? new[] { "http://localhost" };
            
        options.AddPolicy("AllowAngularApp", policy =>
        {
            policy.WithOrigins(allowedOrigins)
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        });
    }
});

// Add controllers
builder.Services.AddControllers();

// Register services
builder.Services.AddScoped<ISampleService, SampleService>();
builder.Services.AddScoped<ITestResultService, TestResultService>();
builder.Services.AddScoped<ICalculationService, CalculationService>();
builder.Services.AddScoped<IEquipmentService, EquipmentService>();
builder.Services.AddScoped<IFileUploadService, FileUploadService>();
builder.Services.AddScoped<ILookupService, LookupService>();
builder.Services.AddScoped<IValidationService, ValidationService>();
builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
builder.Services.AddScoped<IAuthorizationService, AuthorizationService>();
builder.Services.AddScoped<ITransactionService, TransactionService>();
builder.Services.AddScoped<IAuditService, AuditService>();
builder.Services.AddScoped<IParticleAnalysisService, ParticleAnalysisService>();
builder.Services.AddScoped<IParticleTestService, ParticleTestService>();
builder.Services.AddScoped<IEmissionSpectroscopyService, EmissionSpectroscopyService>();

// Critical gap-filling services
builder.Services.AddScoped<ITestSchedulingService, TestSchedulingService>();
builder.Services.AddScoped<ILimitsService, LimitsService>();
builder.Services.AddScoped<IQualificationService, QualificationService>();

// Add HttpContextAccessor for audit service
builder.Services.AddHttpContextAccessor();

// Add HttpClient for migration notification service
builder.Services.AddHttpClient();

// Migration services
builder.Services.AddScoped<LabResultsApi.Services.Migration.IMigrationControlService, LabResultsApi.Services.Migration.MigrationControlService>();
builder.Services.AddScoped<LabResultsApi.Services.Migration.IMigrationLoggingService, LabResultsApi.Services.Migration.MigrationLoggingService>();
builder.Services.AddScoped<LabResultsApi.Services.Migration.IMigrationConfigurationService, LabResultsApi.Services.Migration.MigrationConfigurationService>();
builder.Services.AddScoped<LabResultsApi.Services.Migration.IMigrationPerformanceService, LabResultsApi.Services.Migration.MigrationPerformanceService>();
builder.Services.AddScoped<LabResultsApi.Services.Migration.IMigrationNotificationService, LabResultsApi.Services.Migration.MigrationNotificationService>();

// Placeholder services for migration dependencies (will be implemented in later tasks)
builder.Services.AddScoped<LabResultsApi.Services.Migration.ISqlValidationService>(provider => 
    throw new NotImplementedException("SQL validation service will be implemented in task 3"));

// SSO Migration services
builder.Services.AddScoped<LabResultsApi.Services.Migration.ISsoMigrationService, LabResultsApi.Services.Migration.SsoMigrationService>();
builder.Services.AddScoped<LabResultsApi.Services.Migration.IAuthenticationRemovalService, LabResultsApi.Services.Migration.AuthenticationRemovalService>();



// Performance monitoring services
builder.Services.AddSingleton<IPerformanceMonitoringService, PerformanceMonitoringService>();
builder.Services.AddScoped<IDatabaseIndexingService, DatabaseIndexingService>();

// Raw SQL services - temporarily keep for health checks and legacy endpoints
builder.Services.AddScoped<RawSqlService>();
builder.Services.AddScoped<IRawSqlService>(provider =>
{
    var context = provider.GetRequiredService<LabDbContext>();
    var logger = provider.GetRequiredService<ILogger<OptimizedRawSqlService>>();
    var performanceService = provider.GetRequiredService<IPerformanceMonitoringService>();
    var cache = provider.GetRequiredService<Microsoft.Extensions.Caching.Memory.IMemoryCache>();
    var originalService = provider.GetRequiredService<RawSqlService>();
    
    return new OptimizedRawSqlService(context, logger, performanceService, cache, originalService);
});

// Add memory cache for lookup caching
builder.Services.AddMemoryCache();

// Get performance configuration
var performanceConfig = builder.Configuration.GetSection("Performance").Get<PerformanceConfiguration>() 
    ?? new PerformanceConfiguration();

// Add Entity Framework with performance optimizations
builder.Services.AddDbContext<LabDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"), sqlOptions =>
    {
        if (performanceConfig.DatabaseSettings.EnableRetryOnFailure)
        {
            sqlOptions.EnableRetryOnFailure(
                maxRetryCount: performanceConfig.DatabaseSettings.MaxRetryCount,
                maxRetryDelay: TimeSpan.FromSeconds(performanceConfig.DatabaseSettings.MaxRetryDelaySeconds),
                errorNumbersToAdd: null);
        }
        sqlOptions.CommandTimeout(performanceConfig.DatabaseSettings.CommandTimeoutSeconds);
    });
    
    // Enable sensitive data logging in development
    if (builder.Environment.IsDevelopment())
    {
        options.EnableSensitiveDataLogging();
        options.EnableDetailedErrors();
    }
    
    // Configure query tracking behavior for better performance
    options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
});

// Authentication removed for SSO migration

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Laboratory Test Results API v1");
        c.RoutePrefix = string.Empty; // Set Swagger UI at the app's root
    });
}

app.UseHttpsRedirection();
app.UseCors("AllowAngularApp");

// Authentication middleware removed for SSO migration

// Add global exception handling
app.UseGlobalExceptionHandling();

// Add audit middleware (before performance monitoring)
app.UseMiddleware<AuditMiddleware>();

// Add performance monitoring middleware
app.UseMiddleware<PerformanceMonitoringMiddleware>();

// Map controllers
app.MapControllers();

// Authentication endpoints removed for SSO migration
app.MapSampleEndpoints();
app.MapTestEndpoints();
app.MapEquipmentEndpoints();
app.MapFileUploadEndpoints();
app.MapHistoricalResultsEndpoints();
app.MapEmissionSpectroscopyEndpoints();
app.MapLookupEndpoints();
app.MapParticleAnalysisEndpoints();
app.MapParticleTestEndpoints();
app.MapPerformanceEndpoints();

// Critical gap-filling endpoints
app.MapTestSchedulingEndpoints();
app.MapLimitsEndpoints();
app.MapQualificationEndpoints();

// Health check endpoints
app.MapGet("/health", () => Results.Ok(new { 
    Status = "Healthy", 
    Timestamp = DateTime.UtcNow,
    Environment = app.Environment.EnvironmentName,
    Version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version?.ToString()
}))
.WithName("HealthCheck")
.WithTags("Health")
.AllowAnonymous();

// Database connection test endpoint
app.MapGet("/health/database", async (IRawSqlService rawSqlService) =>
{
    try
    {
        var isConnected = await rawSqlService.TestDatabaseConnectionAsync();
        return isConnected 
            ? Results.Ok(new { Status = "Database Connected", Timestamp = DateTime.UtcNow })
            : Results.Problem("Database connection failed", statusCode: 503);
    }
    catch (Exception ex)
    {
        return Results.Problem($"Database health check error: {ex.Message}", statusCode: 503);
    }
})
.WithName("DatabaseHealthCheck")
.WithTags("Health")
.AllowAnonymous();

// Memory usage health check
app.MapGet("/health/memory", () =>
{
    var gc = GC.GetTotalMemory(false);
    var workingSet = Environment.WorkingSet;
    
    return Results.Ok(new {
        Status = "Memory Check",
        Timestamp = DateTime.UtcNow,
        GCMemory = $"{gc / 1024 / 1024} MB",
        WorkingSet = $"{workingSet / 1024 / 1024} MB"
    });
})
.WithName("MemoryHealthCheck")
.WithTags("Health")
.AllowAnonymous();

// File system health check
app.MapGet("/health/filesystem", (IConfiguration config) =>
{
    try
    {
        var uploadPath = config["FileUpload:BasePath"] ?? "uploads";
        var fullPath = Path.GetFullPath(uploadPath);
        var exists = Directory.Exists(fullPath);
        
        if (!exists)
        {
            Directory.CreateDirectory(fullPath);
        }
        
        // Test write access
        var testFile = Path.Combine(fullPath, "health-check.tmp");
        File.WriteAllText(testFile, "health check");
        File.Delete(testFile);
        
        return Results.Ok(new {
            Status = "File System Accessible",
            Timestamp = DateTime.UtcNow,
            UploadPath = fullPath
        });
    }
    catch (Exception ex)
    {
        return Results.Problem($"File system health check error: {ex.Message}", statusCode: 503);
    }
})
.WithName("FileSystemHealthCheck")
.WithTags("Health")
.AllowAnonymous();

// Comprehensive health check
app.MapGet("/health/detailed", async (IRawSqlService rawSqlService, IConfiguration config) =>
{
    var databaseHealth = await CheckDatabaseHealth(rawSqlService);
    var memoryHealth = CheckMemoryHealth();
    var fileSystemHealth = CheckFileSystemHealth(config);
    
    var healthStatus = new
    {
        Status = "Healthy",
        Timestamp = DateTime.UtcNow,
        Environment = app.Environment.EnvironmentName,
        Version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version?.ToString(),
        Checks = new
        {
            Database = databaseHealth,
            Memory = memoryHealth,
            FileSystem = fileSystemHealth
        }
    };
    
    var hasFailures = databaseHealth.Status != "Healthy" || fileSystemHealth.Status != "Healthy";
    
    return hasFailures ? Results.Json(healthStatus, statusCode: 503) : Results.Ok(healthStatus);
})
.WithName("DetailedHealthCheck")
.WithTags("Health")
.AllowAnonymous();

static async Task<HealthCheckResult> CheckDatabaseHealth(IRawSqlService rawSqlService)
{
    try
    {
        var isConnected = await rawSqlService.TestDatabaseConnectionAsync();
        return new HealthCheckResult 
        { 
            Status = isConnected ? "Healthy" : "Unhealthy", 
            Message = isConnected ? "Connected" : "Connection failed" 
        };
    }
    catch (Exception ex)
    {
        return new HealthCheckResult { Status = "Unhealthy", Message = ex.Message };
    }
}

static HealthCheckResult CheckMemoryHealth()
{
    var gc = GC.GetTotalMemory(false);
    var workingSet = Environment.WorkingSet;
    var gcMB = gc / 1024 / 1024;
    var workingSetMB = workingSet / 1024 / 1024;
    
    return new HealthCheckResult
    {
        Status = workingSetMB > 1000 ? "Warning" : "Healthy",
        Message = $"GC Memory: {gcMB} MB, Working Set: {workingSetMB} MB"
    };
}

static HealthCheckResult CheckFileSystemHealth(IConfiguration config)
{
    try
    {
        var uploadPath = config["FileUpload:BasePath"] ?? "uploads";
        var fullPath = Path.GetFullPath(uploadPath);
        
        if (!Directory.Exists(fullPath))
        {
            Directory.CreateDirectory(fullPath);
        }
        
        var testFile = Path.Combine(fullPath, "health-check.tmp");
        File.WriteAllText(testFile, "health check");
        File.Delete(testFile);
        
        return new HealthCheckResult { Status = "Healthy", Message = $"File system accessible at {fullPath}" };
    }
    catch (Exception ex)
    {
        return new HealthCheckResult { Status = "Unhealthy", Message = ex.Message };
    }
}



app.Run();

// Make Program class accessible for testing
public partial class Program { }
