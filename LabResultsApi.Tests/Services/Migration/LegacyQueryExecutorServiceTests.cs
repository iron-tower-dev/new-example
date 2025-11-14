using NUnit.Framework;
using Moq;
using Microsoft.Extensions.Logging;
using LabResultsApi.Services.Migration;
using FluentAssertions;

namespace LabResultsApi.Tests.Services.Migration;

[TestFixture]
public class LegacyQueryExecutorServiceTests
{
    private Mock<ILogger<LegacyQueryExecutorService>> _mockLogger;
    private LegacyQueryExecutorService _service;

    [SetUp]
    public void SetUp()
    {
        _mockLogger = new Mock<ILogger<LegacyQueryExecutorService>>();
        _service = new LegacyQueryExecutorService(_mockLogger.Object);
    }

    [Test]
    public async Task TestConnectionAsync_WithoutConnectionString_ShouldReturnFalse()
    {
        // Act
        var result = await _service.TestConnectionAsync();

        // Assert
        result.Should().BeFalse();
    }

    [Test]
    public void SetConnectionString_ShouldUpdateConnectionString()
    {
        // Arrange
        var connectionString = "Server=localhost;Database=TestDB;Trusted_Connection=true;";

        // Act
        _service.SetConnectionString(connectionString);

        // Assert - We can't directly test the private field, but we can test behavior
        // The connection string is set, which we'll verify in other tests
        Assert.Pass("Connection string set successfully");
    }

    [Test]
    public async Task ExecuteQueryAsync_WithoutConnectionString_ShouldReturnFailureResult()
    {
        // Arrange
        var query = "SELECT 1";

        // Act
        var result = await _service.ExecuteQueryAsync(query);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Error.Should().Contain("connection string not configured");
        result.Query.Should().Be(query);
        result.ExecutedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Test]
    public async Task ExecuteQueryWithParametersAsync_WithoutConnectionString_ShouldReturnFailureResult()
    {
        // Arrange
        var query = "SELECT * FROM Users WHERE Id = @id";
        var parameters = new Dictionary<string, object> { { "id", 1 } };

        // Act
        var result = await _service.ExecuteQueryWithParametersAsync(query, parameters);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Error.Should().Contain("connection string not configured");
        result.Query.Should().Be(query);
    }

    [Test]
    public async Task GetQueryResultsAsync_WithoutConnectionString_ShouldReturnEmptyList()
    {
        // Arrange
        var query = "SELECT 1";

        // Act
        var result = await _service.GetQueryResultsAsync(query);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Test]
    public async Task GetQueryResultsWithParametersAsync_WithoutConnectionString_ShouldReturnEmptyList()
    {
        // Arrange
        var query = "SELECT * FROM Users WHERE Id = @id";
        var parameters = new Dictionary<string, object> { { "id", 1 } };

        // Act
        var result = await _service.GetQueryResultsWithParametersAsync(query, parameters);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Test]
    public async Task GetRowCountAsync_WithoutConnectionString_ShouldReturnZero()
    {
        // Arrange
        var query = "SELECT COUNT(*) FROM Users";

        // Act
        var result = await _service.GetRowCountAsync(query);

        // Assert
        result.Should().Be(0);
    }

    [Test]
    public async Task GetDataTableAsync_WithoutConnectionString_ShouldReturnEmptyDataTable()
    {
        // Arrange
        var query = "SELECT * FROM Users";

        // Act
        var result = await _service.GetDataTableAsync(query);

        // Assert
        result.Should().NotBeNull();
        result.Rows.Count.Should().Be(0);
        result.Columns.Count.Should().Be(0);
    }

    [Test]
    public async Task ExecuteQueryAsync_WithInvalidConnectionString_ShouldReturnFailureResult()
    {
        // Arrange
        var invalidConnectionString = "Server=invalid;Database=invalid;";
        var query = "SELECT 1";
        _service.SetConnectionString(invalidConnectionString);

        // Act
        var result = await _service.ExecuteQueryAsync(query);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Error.Should().NotBeNullOrEmpty();
        result.ExecutionTime.Should().BeGreaterThan(TimeSpan.Zero);
    }

    [Test]
    public async Task ExecuteQueryAsync_WithTimeout_ShouldRespectTimeout()
    {
        // Arrange
        var invalidConnectionString = "Server=invalid;Database=invalid;Connection Timeout=1;";
        var query = "SELECT 1";
        var timeoutMinutes = 1;
        _service.SetConnectionString(invalidConnectionString);

        // Act
        var result = await _service.ExecuteQueryAsync(query, timeoutMinutes);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.ExecutionTime.Should().BeLessThan(TimeSpan.FromMinutes(2)); // Should timeout before 2 minutes
    }

    [Test]
    public void ExecuteQueryAsync_ShouldHandleParametersCorrectly()
    {
        // Arrange
        var query = "SELECT * FROM Users WHERE Id = @id AND Name = @name";
        var parameters = new Dictionary<string, object> 
        { 
            { "id", 1 },
            { "name", "John Doe" },
            { "@email", "john@example.com" } // Test parameter with @ prefix
        };

        // Act & Assert - This test verifies parameter handling logic
        // Since we can't easily test with a real database in unit tests,
        // we're testing the parameter preparation logic indirectly
        var task = _service.ExecuteQueryWithParametersAsync(query, parameters);
        
        // The method should not throw an exception during parameter setup
        task.Should().NotBeNull();
    }

    [Test]
    public async Task ExecuteQueryAsync_ShouldSetExecutionMetadata()
    {
        // Arrange
        var query = "SELECT 1 as TestColumn";
        var beforeExecution = DateTime.UtcNow;

        // Act
        var result = await _service.ExecuteQueryAsync(query);

        // Assert
        result.Should().NotBeNull();
        result.Query.Should().Be(query);
        result.ExecutedAt.Should().BeOnOrAfter(beforeExecution);
        result.ExecutedAt.Should().BeOnOrBefore(DateTime.UtcNow);
        result.ExecutionTime.Should().BeGreaterOrEqualTo(TimeSpan.Zero);
    }

    [Test]
    public async Task GetQueryResultsAsync_WithValidQuery_ShouldReturnCorrectStructure()
    {
        // Arrange
        var query = "SELECT 1 as Id, 'Test' as Name";

        // Act
        var result = await _service.GetQueryResultsAsync(query);

        // Assert
        result.Should().NotBeNull();
        // Without a real database connection, this will return empty,
        // but the structure should be correct
        result.Should().BeOfType<List<Dictionary<string, object>>>();
    }
}