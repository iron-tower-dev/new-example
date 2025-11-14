using NUnit.Framework;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using LabResultsApi.Data;
using LabResultsApi.Tests.TestData;
using LabResultsApi.Services;
using LabResultsApi.Tests.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Memory;
using Moq;

namespace LabResultsApi.Tests;

[TestFixture]
public abstract class TestBase
{
    protected IServiceProvider ServiceProvider { get; private set; } = null!;
    protected LabDbContext Context { get; private set; } = null!;

    [OneTimeSetUp]
    public virtual async Task OneTimeSetUp()
    {
        var services = new ServiceCollection();
        ConfigureServices(services);
        ServiceProvider = services.BuildServiceProvider();
        
        // Get the context from the service provider so all services use the same context
        Context = ServiceProvider.GetRequiredService<LabDbContext>();
        
        // Seed the data directly using the same context
        await Context.Database.EnsureCreatedAsync();
        TestDataSeeder.SeedTestData(Context);
    }

    [OneTimeTearDown]
    public virtual async Task OneTimeTearDown()
    {
        if (Context != null)
        {
            TestDataSeeder.ClearTestData(Context);
            await Context.Database.EnsureDeletedAsync();
        }
        
        if (ServiceProvider is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }

    [SetUp]
    public virtual async Task SetUp()
    {
        // Reset data before each test
        TestDataSeeder.ClearTestData(Context);
        TestDataSeeder.SeedTestData(Context);
    }

    [TearDown]
    public virtual Task TearDown()
    {
        // Clean up after each test if needed
        return Task.CompletedTask;
    }

    protected virtual void ConfigureServices(IServiceCollection services)
    {
        // Add DbContext with in-memory database
        services.AddDbContext<LabDbContext>(options =>
            options.UseInMemoryDatabase($"TestDb_{GetType().Name}_{Guid.NewGuid()}"));

        // Add memory cache
        services.AddMemoryCache();

        // Add logging
        services.AddLogging(builder => builder.AddConsole());

        // Register services with mocked dependencies where needed
        RegisterServices(services);
    }

    protected virtual void RegisterServices(IServiceCollection services)
    {
        // Register all application services
        services.AddScoped<ISampleService, SampleService>();
        services.AddScoped<ITestResultService, TestResultService>();
        services.AddScoped<ICalculationService, CalculationService>();
        services.AddScoped<IEquipmentService, EquipmentService>();
        services.AddScoped<IFileUploadService, FileUploadService>();
        services.AddScoped<ILookupService, LookupService>();
        services.AddScoped<IValidationService, ValidationService>();
        services.AddScoped<IAuthenticationService, AuthenticationService>();
        // Use a test-compatible version of RawSqlService for in-memory database
        services.AddScoped<IRawSqlService, TestRawSqlService>();
        services.AddSingleton<IPerformanceMonitoringService, PerformanceMonitoringService>();

        // Add AutoMapper
        services.AddAutoMapper(typeof(Program));
    }

    protected T GetService<T>() where T : notnull
    {
        return ServiceProvider.GetRequiredService<T>();
    }

    protected Mock<T> GetMockService<T>() where T : class
    {
        return new Mock<T>();
    }
}

[TestFixture]
public abstract class ServiceTestBase<TService> : TestBase where TService : class
{
    protected TService Service { get; private set; } = null!;

    [SetUp]
    public override async Task SetUp()
    {
        await base.SetUp();
        Service = GetService<TService>();
    }
}

[TestFixture]
public abstract class IntegrationTestBase : TestBase
{
    protected HttpClient Client { get; private set; } = null!;
    private Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactory<Program>? _factory;

    [OneTimeSetUp]
    public override async Task OneTimeSetUp()
    {
        await base.OneTimeSetUp();
        
        _factory = new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // Remove the real database context
                    var descriptor = services.SingleOrDefault(
                        d => d.ServiceType == typeof(DbContextOptions<LabDbContext>));
                    if (descriptor != null)
                        services.Remove(descriptor);

                    // Add test database context
                    services.AddDbContext<LabDbContext>(options =>
                        options.UseInMemoryDatabase($"IntegrationTestDb_{GetType().Name}"));
                });
            });

        Client = _factory.CreateClient();
    }

    [OneTimeTearDown]
    public override async Task OneTimeTearDown()
    {
        Client?.Dispose();
        _factory?.Dispose();
        await base.OneTimeTearDown();
    }
}