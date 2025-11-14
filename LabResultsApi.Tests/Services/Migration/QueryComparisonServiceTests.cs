using NUnit.Framework;
using Moq;
using Microsoft.Extensions.Logging;
using LabResultsApi.Services.Migration;
using LabResultsApi.Services;
using LabResultsApi.Models.Migration;
using FluentAssertions;

namespace LabResultsApi.Tests.Services.Migration;

[TestFixture]
public class QueryComparisonServiceTests
{
    private Mock<IRawSqlService> _mockCurrentDbService;
    private Mock<ILegacyQueryExecutorService> _mockLegacyDbService;
    private Mock<ILogger<QueryComparisonService>> _mockLogger;
    private QueryComparisonService _service;

    [SetUp]
    public void SetUp()
    {
        _mockCurrentDbService = new Mock<IRawSqlService>();
        _mockLegacyDbService = new Mock<ILegacyQueryExecutorService>();
        _mockLogger = new Mock<ILogger<QueryComparisonService>>();

        _service = new QueryComparisonService(
            _mockCurrentDbService.Object,
            _mockLegacyDbService.Object,
            _mockLogger.Object);
    }

    [Test]
    public async Task CompareDataAsync_WithIdenticalData_ShouldReturnNoDiscrepancies()
    {
        // Arrange
        var currentData = new List<Dictionary<string, object>>
        {
            new() { { "Id", 1 }, { "Name", "John" }, { "Age", 30 } },
            new() { { "Id", 2 }, { "Name", "Jane" }, { "Age", 25 } }
        };

        var legacyData = new List<Dictionary<string, object>>
        {
            new() { { "Id", 1 }, { "Name", "John" }, { "Age", 30 } },
            new() { { "Id", 2 }, { "Name", "Jane" }, { "Age", 25 } }
        };

        // Act
        var discrepancies = await _service.CompareDataAsync(currentData, legacyData, "TestQuery");

        // Assert
        discrepancies.Should().BeEmpty();
    }

    [Test]
    public async Task CompareDataAsync_WithDifferentRowCounts_ShouldReturnRowCountDiscrepancy()
    {
        // Arrange
        var currentData = new List<Dictionary<string, object>>
        {
            new() { { "Id", 1 }, { "Name", "John" } }
        };

        var legacyData = new List<Dictionary<string, object>>
        {
            new() { { "Id", 1 }, { "Name", "John" } },
            new() { { "Id", 2 }, { "Name", "Jane" } }
        };

        // Act
        var discrepancies = await _service.CompareDataAsync(currentData, legacyData, "TestQuery");

        // Assert
        discrepancies.Should().NotBeEmpty();
        discrepancies.Should().Contain(d => d.FieldName == "RowCount" && d.Type == DiscrepancyType.ValueMismatch);
        
        var rowCountDiscrepancy = discrepancies.First(d => d.FieldName == "RowCount");
        rowCountDiscrepancy.CurrentValue.Should().Be(1);
        rowCountDiscrepancy.LegacyValue.Should().Be(2);
    }

    [Test]
    public async Task CompareDataAsync_WithValueMismatches_ShouldReturnValueDiscrepancies()
    {
        // Arrange
        var currentData = new List<Dictionary<string, object>>
        {
            new() { { "Id", 1 }, { "Name", "John" }, { "Age", 30 } }
        };

        var legacyData = new List<Dictionary<string, object>>
        {
            new() { { "Id", 1 }, { "Name", "Johnny" }, { "Age", 31 } }
        };

        // Act
        var discrepancies = await _service.CompareDataAsync(currentData, legacyData, "TestQuery");

        // Assert
        discrepancies.Should().HaveCount(2); // Name and Age mismatches
        discrepancies.Should().Contain(d => d.FieldName == "Name" && d.Type == DiscrepancyType.ValueMismatch);
        discrepancies.Should().Contain(d => d.FieldName == "Age" && d.Type == DiscrepancyType.ValueMismatch);
    }

    [Test]
    public async Task CompareDataAsync_WithMissingFields_ShouldReturnFieldDiscrepancies()
    {
        // Arrange
        var currentData = new List<Dictionary<string, object>>
        {
            new() { { "Id", 1 }, { "Name", "John" } }
        };

        var legacyData = new List<Dictionary<string, object>>
        {
            new() { { "Id", 1 }, { "Name", "John" }, { "Age", 30 } }
        };

        // Act
        var discrepancies = await _service.CompareDataAsync(currentData, legacyData, "TestQuery");

        // Assert
        discrepancies.Should().Contain(d => d.FieldName == "Age" && d.Type == DiscrepancyType.MissingInCurrent);
    }

    [Test]
    public async Task CompareDataAsync_WithEmptyData_ShouldReturnNoDiscrepancies()
    {
        // Arrange
        var currentData = new List<Dictionary<string, object>>();
        var legacyData = new List<Dictionary<string, object>>();

        // Act
        var discrepancies = await _service.CompareDataAsync(currentData, legacyData, "TestQuery");

        // Assert
        discrepancies.Should().BeEmpty();
    }

    [Test]
    public async Task CompareDataAsync_WithExtraRowsInCurrent_ShouldReturnMissingInLegacyDiscrepancies()
    {
        // Arrange
        var currentData = new List<Dictionary<string, object>>
        {
            new() { { "Id", 1 }, { "Name", "John" } },
            new() { { "Id", 2 }, { "Name", "Jane" } }
        };

        var legacyData = new List<Dictionary<string, object>>
        {
            new() { { "Id", 1 }, { "Name", "John" } }
        };

        // Act
        var discrepancies = await _service.CompareDataAsync(currentData, legacyData, "TestQuery");

        // Assert
        discrepancies.Should().Contain(d => d.Type == DiscrepancyType.MissingInLegacy && d.FieldName == "Row");
    }

    [Test]
    public void AreValuesEqual_WithIdenticalValues_ShouldReturnTrue()
    {
        // Arrange & Act & Assert
        _service.AreValuesEqual("test", "test", "Name").Should().BeTrue();
        _service.AreValuesEqual(123, 123, "Id").Should().BeTrue();
        _service.AreValuesEqual(null, null, "Field").Should().BeTrue();
        _service.AreValuesEqual(DBNull.Value, DBNull.Value, "Field").Should().BeTrue();
    }

    [Test]
    public void AreValuesEqual_WithDifferentValues_ShouldReturnFalse()
    {
        // Arrange & Act & Assert
        _service.AreValuesEqual("test1", "test2", "Name").Should().BeFalse();
        _service.AreValuesEqual(123, 124, "Id").Should().BeFalse();
        _service.AreValuesEqual(null, "value", "Field").Should().BeFalse();
        _service.AreValuesEqual("value", null, "Field").Should().BeFalse();
    }

    [Test]
    public void AreValuesEqual_WithNumericValues_ShouldHandleSmallDifferences()
    {
        // Arrange & Act & Assert
        _service.AreValuesEqual(1.0001m, 1.0002m, "amount").Should().BeTrue(); // Within tolerance
        _service.AreValuesEqual(1.0m, 1.1m, "amount").Should().BeFalse(); // Outside tolerance
    }

    [Test]
    public void AreValuesEqual_WithCaseInsensitiveStrings_ShouldReturnTrue()
    {
        // Arrange & Act & Assert
        _service.AreValuesEqual("Test", "test", "Name").Should().BeTrue();
        _service.AreValuesEqual("HELLO", "hello", "Greeting").Should().BeTrue();
    }

    [Test]
    public void GenerateRowIdentifier_WithKeyFields_ShouldGenerateCorrectIdentifier()
    {
        // Arrange
        var row = new Dictionary<string, object>
        {
            { "Id", 1 },
            { "Name", "John" },
            { "Age", 30 }
        };
        var keyFields = new List<string> { "Id", "Name" };

        // Act
        var identifier = _service.GenerateRowIdentifier(row, keyFields);

        // Assert
        identifier.Should().Be("Id=1, Name=John");
    }

    [Test]
    public void GenerateRowIdentifier_WithoutKeyFields_ShouldUseFirstThreeFields()
    {
        // Arrange
        var row = new Dictionary<string, object>
        {
            { "Id", 1 },
            { "Name", "John" },
            { "Age", 30 },
            { "Email", "john@example.com" }
        };
        var keyFields = new List<string>();

        // Act
        var identifier = _service.GenerateRowIdentifier(row, keyFields);

        // Assert
        identifier.Should().Contain("Id=1");
        identifier.Should().Contain("Name=John");
        identifier.Should().Contain("Age=30");
        identifier.Should().NotContain("Email");
    }

    [Test]
    public void GenerateRowIdentifier_WithEmptyRow_ShouldReturnUnknown()
    {
        // Arrange
        var row = new Dictionary<string, object>();
        var keyFields = new List<string> { "Id" };

        // Act
        var identifier = _service.GenerateRowIdentifier(row, keyFields);

        // Assert
        identifier.Should().Be("Unknown");
    }

    [Test]
    public async Task ValidateQueriesAsync_WithMultipleQueries_ShouldReturnValidationResult()
    {
        // Arrange
        var queries = new Dictionary<string, QueryPair>
        {
            {
                "Query1",
                new QueryPair
                {
                    CurrentQuery = "SELECT 1",
                    LegacyQuery = "SELECT 1",
                    Parameters = null
                }
            },
            {
                "Query2",
                new QueryPair
                {
                    CurrentQuery = "SELECT 2",
                    LegacyQuery = "SELECT 2",
                    Parameters = new Dictionary<string, object> { { "id", 1 } }
                }
            }
        };

        // Mock legacy service to return successful results
        _mockLegacyDbService.Setup(x => x.ExecuteQueryAsync(It.IsAny<string>(), It.IsAny<int>()))
            .ReturnsAsync(new QueryExecutionResult
            {
                Success = true,
                Data = new List<Dictionary<string, object>>(),
                RowCount = 0,
                ExecutionTime = TimeSpan.FromMilliseconds(100)
            });

        _mockLegacyDbService.Setup(x => x.ExecuteQueryWithParametersAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>(), It.IsAny<int>()))
            .ReturnsAsync(new QueryExecutionResult
            {
                Success = true,
                Data = new List<Dictionary<string, object>>(),
                RowCount = 0,
                ExecutionTime = TimeSpan.FromMilliseconds(100)
            });

        // Act
        var result = await _service.ValidateQueriesAsync(queries);

        // Assert
        result.Should().NotBeNull();
        result.QueriesValidated.Should().Be(2);
        result.StartTime.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
        result.EndTime.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
        result.Duration.Should().BeGreaterThan(TimeSpan.Zero);
        result.Results.Should().HaveCount(2);
    }

    [Test]
    public async Task PerformanceComparisonAsync_ShouldExecuteQueriesMultipleTimes()
    {
        // Arrange
        var queryName = "TestQuery";
        var currentQuery = "SELECT 1";
        var legacyQuery = "SELECT 1";

        _mockLegacyDbService.Setup(x => x.ExecuteQueryAsync(It.IsAny<string>(), It.IsAny<int>()))
            .ReturnsAsync(new QueryExecutionResult
            {
                Success = true,
                ExecutionTime = TimeSpan.FromMilliseconds(50)
            });

        // Act
        var result = await _service.ComparePerformanceAsync(queryName, currentQuery, legacyQuery);

        // Assert
        result.Should().NotBeNull();
        result.QueryName.Should().Be(queryName);
        result.CurrentExecutionTime.Should().BeGreaterOrEqualTo(TimeSpan.Zero);
        result.LegacyExecutionTime.Should().BeGreaterOrEqualTo(TimeSpan.Zero);

        // Verify that the legacy service was called multiple times (3 times for averaging)
        _mockLegacyDbService.Verify(x => x.ExecuteQueryAsync(legacyQuery, It.IsAny<int>()), Times.Exactly(3));
    }

    [Test]
    public async Task CompareDataAsync_WithNullValues_ShouldHandleCorrectly()
    {
        // Arrange
        var currentData = new List<Dictionary<string, object>>
        {
            new() { { "Id", 1 }, { "Name", null }, { "Age", 30 } }
        };

        var legacyData = new List<Dictionary<string, object>>
        {
            new() { { "Id", 1 }, { "Name", null }, { "Age", 30 } }
        };

        // Act
        var discrepancies = await _service.CompareDataAsync(currentData, legacyData, "TestQuery");

        // Assert
        discrepancies.Should().BeEmpty();
    }

    [Test]
    public async Task CompareDataAsync_WithDBNullValues_ShouldHandleCorrectly()
    {
        // Arrange
        var currentData = new List<Dictionary<string, object>>
        {
            new() { { "Id", 1 }, { "Name", DBNull.Value }, { "Age", 30 } }
        };

        var legacyData = new List<Dictionary<string, object>>
        {
            new() { { "Id", 1 }, { "Name", DBNull.Value }, { "Age", 30 } }
        };

        // Act
        var discrepancies = await _service.CompareDataAsync(currentData, legacyData, "TestQuery");

        // Assert
        discrepancies.Should().BeEmpty();
    }
}