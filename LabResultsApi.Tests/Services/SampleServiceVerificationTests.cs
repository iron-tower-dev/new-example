using Xunit;
using Microsoft.EntityFrameworkCore;
using LabResultsApi.Data;
using LabResultsApi.Services;
using LabResultsApi.Models;
using Microsoft.Extensions.Logging;
using Moq;

namespace LabResultsApi.Tests.Services;

public class SampleServiceVerificationTests : IDisposable
{
    private readonly LabDbContext _context;
    private readonly SampleService _sampleService;
    private readonly Mock<IRawSqlService> _mockRawSqlService;
    private readonly Mock<ILogger<SampleService>> _mockLogger;

    public SampleServiceVerificationTests()
    {
        // Setup in-memory database
        var options = new DbContextOptionsBuilder<LabDbContext>()
            .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
            .Options;

        _context = new LabDbContext(options);
        _mockRawSqlService = new Mock<IRawSqlService>();
        _mockLogger = new Mock<ILogger<SampleService>>();
        
        _sampleService = new SampleService(_context, _mockRawSqlService.Object, _mockLogger.Object);
        
        SeedTestData();
    }

    private void SeedTestData()
    {
        // Add test sample with all fields
        var sample = new Sample
        {
            Id = 1,
            TagNumber = "TEST001",
            Component = "001",
            Location = "001",
            LubeType = "Test Oil",
            WoNumber = "WO12345",
            SampleDate = DateTime.Now.AddDays(-5),
            ReceivedOn = DateTime.Now.AddDays(-4),
            SampledBy = "Test User",
            Status = 250, // Active status
            SiteId = 1,
            ResultsReviewDate = DateTime.Now.AddDays(-1),
            ResultsAvailDate = DateTime.Now.AddDays(-2)
        };
        
        _context.UsedLubeSamples.Add(sample);

        // Add Lube_Sampling_Point for quality class
        var lubeSamplingPoint = new LubeSamplingPoint
        {
            Id = 1,
            TagNumber = "TEST001",
            Component = "001",
            Location = "001",
            QualityClass = "A"
        };
        
        _context.LubeSamplingPoints.Add(lubeSamplingPoint);
        
        _context.SaveChanges();
    }

    [Fact]
    public async Task GetSampleAsync_ReturnsAllFields()
    {
        // Act
        var result = await _sampleService.GetSampleAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal("TEST001", result.TagNumber);
        Assert.Equal("001", result.Component);
        Assert.Equal("001", result.Location);
        Assert.Equal("Test Oil", result.LubeType);
        Assert.Equal("WO12345", result.WoNumber);
        Assert.Equal((short)250, result.Status);
        Assert.Equal(1, result.SiteId);
        Assert.NotNull(result.SampleDate);
        Assert.NotNull(result.ReceivedOn);
        Assert.Equal("Test User", result.SampledBy);
        Assert.NotNull(result.ResultsReviewDate);
        Assert.NotNull(result.ResultsAvailDate);
    }

    [Fact]
    public async Task GetSampleAsync_PopulatesQualityClassFromJoin()
    {
        // Act
        var result = await _sampleService.GetSampleAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("A", result.QualityClass); // Should come from Lube_Sampling_Point
    }

    [Fact]
    public async Task GetSamplesAsync_FiltersByStatus()
    {
        // Arrange
        var filter = new DTOs.SampleFilterDto
        {
            Status = 250
        };

        // Act
        var results = await _sampleService.GetSamplesAsync(filter);

        // Assert
        Assert.NotEmpty(results);
        Assert.All(results, r => Assert.Equal((short)250, r.Status));
    }

    [Fact]
    public async Task Sample_Model_HasCorrectDataTypes()
    {
        // Arrange
        var sample = await _context.UsedLubeSamples.FirstOrDefaultAsync();

        // Assert
        Assert.NotNull(sample);
        Assert.IsType<short?>(sample.Status); // Should be short, not string
        Assert.IsType<int?>(sample.SiteId);
        Assert.IsType<byte?>(sample.CmptSelectFlag);
        Assert.IsType<byte?>(sample.NewUsedFlag);
    }

    [Fact]
    public async Task LubeSamplingPoint_Model_Exists()
    {
        // Act
        var lubeSamplingPoint = await _context.LubeSamplingPoints.FirstOrDefaultAsync();

        // Assert
        Assert.NotNull(lubeSamplingPoint);
        Assert.Equal("TEST001", lubeSamplingPoint.TagNumber);
        Assert.Equal("A", lubeSamplingPoint.QualityClass);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
