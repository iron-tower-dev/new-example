using LabResultsApi.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace LabResultsApi.Tests.TestData;

public class TestDataManager : IDisposable
{
    private readonly IServiceScope _scope;
    private readonly LabDbContext _context;

    public TestDataManager(IServiceProvider serviceProvider)
    {
        _scope = serviceProvider.CreateScope();
        _context = _scope.ServiceProvider.GetRequiredService<LabDbContext>();
    }

    public async Task SetupTestDataAsync()
    {
        await _context.Database.EnsureCreatedAsync();
        TestDataSeeder.SeedTestData(_context);
    }

    public async Task CleanupTestDataAsync()
    {
        TestDataSeeder.ClearTestData(_context);
        await _context.Database.EnsureDeletedAsync();
    }

    public async Task ResetTestDataAsync()
    {
        TestDataSeeder.ClearTestData(_context);
        TestDataSeeder.SeedTestData(_context);
    }

    public LabDbContext GetContext() => _context;

    public void Dispose()
    {
        _context?.Dispose();
        _scope?.Dispose();
    }
}

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class TestDataSetupAttribute : Attribute
{
    public bool SeedEmissionSpectroscopy { get; set; } = false;
    public bool SeedParticleAnalysis { get; set; } = false;
    public bool SeedFileUploads { get; set; } = false;
}

public static class TestDataExtensions
{
    public static async Task<T> WithTestDataAsync<T>(this T test, Func<TestDataManager, Task> action) where T : class
    {
        var serviceProvider = TestServiceProvider.GetServiceProvider();
        using var dataManager = new TestDataManager(serviceProvider);
        
        await dataManager.SetupTestDataAsync();
        
        try
        {
            await action(dataManager);
        }
        finally
        {
            await dataManager.CleanupTestDataAsync();
        }
        
        return test;
    }
}

public static class TestServiceProvider
{
    private static IServiceProvider? _serviceProvider;

    public static IServiceProvider GetServiceProvider()
    {
        if (_serviceProvider == null)
        {
            var services = new ServiceCollection();
            
            services.AddDbContext<LabDbContext>(options =>
                options.UseInMemoryDatabase($"TestDb_{Guid.NewGuid()}"));
            
            _serviceProvider = services.BuildServiceProvider();
        }
        
        return _serviceProvider;
    }

    public static void Reset()
    {
        _serviceProvider = null;
    }
}