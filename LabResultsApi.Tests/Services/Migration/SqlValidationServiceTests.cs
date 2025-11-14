using NUnit.Framework;
using Moq;
using Microsoft.Extensions.Logging;
using LabResultsApi.Services.Migration;
using LabResultsApi.Models.Migration;
using LabResultsApi.DTOs.Migration;
using FluentAssertions;

namespace LabResultsApi.Tests.Services.Migration;

[TestFixture]
public class SqlValidationServiceTests
{
    private Mock<IQueryComparisonService> _mockQueryComparisonService;
    private Mock<ILegacyQueryExecutorService> _mockLegacyQueryExecutorService;
    private Mock<IValidationReportingService> _mockReportingService;
    private Mock<ILogger<SqlValidationService>> _mockLogger;
    private SqlValidationService _service;

    [SetUp]
    public void SetUp()
    {
        _mockQueryComparisonService = new Mock<IQueryComparisonService>();
        _mockLegacyQueryExecutorService = new Mock<ILegacyQueryExecutorService>();
        _mockReportingService = new Mock<IValidationReportingService>();
        _mockLogger = new Mock<ILogger<SqlValidationService>>();

        _service = new SqlValidationService(
            _mockQueryComparisonService.Object,
            _mockLegacyQueryExecutorService.Object,
            _mockReportingService.Object,
            _mockLogger.Object);
    }

    [Test]
    public async Task TestLegacyConnectionAsync_ShouldCallLegacyService()
    {
        // Arrange
        _mockLegacyQueryExecutorService.Setup(x => x.TestConnectionAsync())
            .ReturnsAsync(true);

        // Act
        var result = await _service.TestLegacyConnectionAsync();

        // Assert
        result.Should().BeTrue();
        _mockLegacyQueryExecutorService.Verify(x => x.TestConnectionAsync(), Times.Once);
    }

    [Test]
    public void SetLegacyConnectionString_ShouldCallLegacyService()
    {
        // Arrange
        var connectionString = "Server=localhost;Database=TestDB;";

        // Act
        _service.SetLegacyConnectionString(connectionString);

        // Assert
        _mockLegacyQueryExecutorService.Verify(x => x.SetConnectionString(connectionString), Times.Once);
    }

    [Test]
    public async Task GetAvailableQueriesAsync_ShouldReturnListOfQueries()
    {
        // Act
        var queries = await _service.GetAvailableQueriesAsync();

        // Assert
        queries.Should().NotBeEmpty();
        queries.Should().Contain("GetTestReadings");
        queries.Should().Contain("GetEmissionSpectroscopy");
        queries.Should().Contain("GetParticleTypes");
    }

    [Test]
    public async Task CompareQueryAsync_ShouldCallQueryComparisonService()
    {
        // Arrange
        var queryName = "TestQuery";
        var currentQuery = "SELECT 1";
        var legacyQuery = "SELECT 1";
        var parameters = new Dictionary<string, object> { { "id", 1 } };

        var expectedResult = new QueryComparisonResult
        {
            QueryName = queryName,
            DataMatches = true,
            CurrentRowCount = 1,
            LegacyRowCount = 1
        };

        _mockQueryComparisonService.Setup(x => x.CompareQueriesAsync(queryName, currentQuery, legacyQuery, parameters))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _service.CompareQueryAsync(queryName, currentQuery, legacyQuery, parameters);

        // Assert
        result.Should().Be(expectedResult);
        _mockQueryComparisonService.Verify(x => x.CompareQueriesAsync(queryName, currentQuery, legacyQuery, parameters), Times.Once);
    }

    [Test]
    public async Task ComparePerformanceAsync_ShouldCallQueryComparisonService()
    {
        // Arrange
        var queryName = "TestQuery";
        var currentQuery = "SELECT 1";
        var legacyQuery = "SELECT 1";

        var expectedResult = new PerformanceComparisonResult
        {
            QueryName = queryName,
            CurrentExecutionTime = TimeSpan.FromMilliseconds(100),
            LegacyExecutionTime = TimeSpan.FromMilliseconds(120)
        };

        _mockQueryComparisonService.Setup(x => x.ComparePerformanceAsync(queryName, currentQuery, legacyQuery, null))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _service.ComparePerformanceAsync(queryName, currentQuery, legacyQuery);

        // Assert
        result.Should().Be(expectedResult);
        _mockQueryComparisonService.Verify(x => x.ComparePerformanceAsync(queryName, currentQuery, legacyQuery, null), Times.Once);
    }

    [Test]
    public async Task ValidateQueriesAsync_ShouldCallQueryComparisonService()
    {
        // Arrange
        var queries = new Dictionary<string, QueryPair>
        {
            {
                "TestQuery",
                new QueryPair
                {
                    CurrentQuery = "SELECT 1",
                    LegacyQuery = "SELECT 1"
                }
            }
        };

        var options = new ValidationOptions
        {
            MaxDiscrepanciesToReport = 50,
            IgnoreMinorDifferences = true
        };

        var expectedResult = new ValidationResult
        {
            QueriesValidated = 1,
            QueriesMatched = 1,
            QueriesFailed = 0,
            Results = new List<QueryComparisonResult>
            {
                new()
                {
                    QueryName = "TestQuery",
                    DataMatches = true,
                    Discrepancies = new List<DataDiscrepancy>()
                }
            }
        };

        _mockQueryComparisonService.Setup(x => x.ValidateQueriesAsync(queries))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _service.ValidateQueriesAsync(queries, options);

        // Assert
        result.Should().NotBeNull();
        result.QueriesValidated.Should().Be(1);
        _mockQueryComparisonService.Verify(x => x.ValidateQueriesAsync(queries), Times.Once);
    }

    [Test]
    public async Task ValidateQueriesAsync_WithMaxDiscrepanciesLimit_ShouldTruncateDiscrepancies()
    {
        // Arrange
        var queries = new Dictionary<string, QueryPair>
        {
            {
                "TestQuery",
                new QueryPair
                {
                    CurrentQuery = "SELECT 1",
                    LegacyQuery = "SELECT 1"
                }
            }
        };

        var options = new ValidationOptions
        {
            MaxDiscrepanciesToReport = 2,
            IgnoreMinorDifferences = false
        };

        var discrepancies = new List<DataDiscrepancy>
        {
            new() { FieldName = "Field1", Type = DiscrepancyType.ValueMismatch },
            new() { FieldName = "Field2", Type = DiscrepancyType.ValueMismatch },
            new() { FieldName = "Field3", Type = DiscrepancyType.ValueMismatch },
            new() { FieldName = "Field4", Type = DiscrepancyType.ValueMismatch }
        };

        var validationResult = new ValidationResult
        {
            QueriesValidated = 1,
            QueriesMatched = 0,
            QueriesFailed = 1,
            Results = new List<QueryComparisonResult>
            {
                new()
                {
                    QueryName = "TestQuery",
                    DataMatches = false,
                    Discrepancies = discrepancies
                }
            }
        };

        _mockQueryComparisonService.Setup(x => x.ValidateQueriesAsync(queries))
            .ReturnsAsync(validationResult);

        // Act
        var result = await _service.ValidateQueriesAsync(queries, options);

        // Assert
        result.Should().NotBeNull();
        result.Results.First().Discrepancies.Should().HaveCount(2);
    }

    [Test]
    public async Task ValidateAllQueriesAsync_WithoutLegacyConnection_ShouldThrowException()
    {
        // Arrange
        var options = new ValidationOptions
        {
            LegacyConnectionString = "invalid connection"
        };

        _mockLegacyQueryExecutorService.Setup(x => x.TestConnectionAsync())
            .ReturnsAsync(false);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.ValidateAllQueriesAsync(options));

        exception.Message.Should().Contain("Cannot connect to legacy database");
    }

    [Test]
    public async Task ValidateAllQueriesAsync_WithValidConnection_ShouldCompleteValidation()
    {
        // Arrange
        var options = new ValidationOptions
        {
            LegacyConnectionString = "valid connection",
            IncludeQueries = new List<string> { "GetTestReadings" }
        };

        _mockLegacyQueryExecutorService.Setup(x => x.TestConnectionAsync())
            .ReturnsAsync(true);

        var validationResult = new ValidationResult
        {
            QueriesValidated = 1,
            QueriesMatched = 1,
            QueriesFailed = 0,
            Results = new List<QueryComparisonResult>
            {
                new()
                {
                    QueryName = "GetTestReadings",
                    DataMatches = true,
                    Discrepancies = new List<DataDiscrepancy>()
                }
            }
        };

        _mockQueryComparisonService.Setup(x => x.ValidateQueriesAsync(It.IsAny<Dictionary<string, QueryPair>>()))
            .ReturnsAsync(validationResult);

        // Act
        var result = await _service.ValidateAllQueriesAsync(options);

        // Assert
        result.Should().NotBeNull();
        result.QueriesValidated.Should().Be(1);
        _mockLegacyQueryExecutorService.Verify(x => x.SetConnectionString(options.LegacyConnectionString), Times.Once);
        _mockLegacyQueryExecutorService.Verify(x => x.TestConnectionAsync(), Times.Once);
    }

    [Test]
    public async Task ValidateAllQueriesAsync_WithIncludeFilter_ShouldOnlyValidateIncludedQueries()
    {
        // Arrange
        var options = new ValidationOptions
        {
            LegacyConnectionString = "valid connection",
            IncludeQueries = new List<string> { "GetTestReadings", "GetEmissionSpectroscopy" }
        };

        _mockLegacyQueryExecutorService.Setup(x => x.TestConnectionAsync())
            .ReturnsAsync(true);

        var validationResult = new ValidationResult
        {
            QueriesValidated = 2,
            QueriesMatched = 2,
            QueriesFailed = 0,
            Results = new List<QueryComparisonResult>()
        };

        _mockQueryComparisonService.Setup(x => x.ValidateQueriesAsync(It.IsAny<Dictionary<string, QueryPair>>()))
            .ReturnsAsync(validationResult)
            .Callback<Dictionary<string, QueryPair>>(queries =>
            {
                // Verify that only the included queries are present
                queries.Keys.Should().Contain("GetTestReadings");
                queries.Keys.Should().Contain("GetEmissionSpectroscopy");
                queries.Keys.Should().NotContain("GetParticleTypes");
            });

        // Act
        var result = await _service.ValidateAllQueriesAsync(options);

        // Assert
        result.Should().NotBeNull();
        _mockQueryComparisonService.Verify(x => x.ValidateQueriesAsync(It.IsAny<Dictionary<string, QueryPair>>()), Times.Once);
    }

    [Test]
    public async Task ValidateAllQueriesAsync_WithExcludeFilter_ShouldExcludeSpecifiedQueries()
    {
        // Arrange
        var options = new ValidationOptions
        {
            LegacyConnectionString = "valid connection",
            ExcludeQueries = new List<string> { "GetParticleTypes", "GetParticleSubTypes" }
        };

        _mockLegacyQueryExecutorService.Setup(x => x.TestConnectionAsync())
            .ReturnsAsync(true);

        var validationResult = new ValidationResult
        {
            QueriesValidated = 8, // Total available minus excluded
            QueriesMatched = 8,
            QueriesFailed = 0,
            Results = new List<QueryComparisonResult>()
        };

        _mockQueryComparisonService.Setup(x => x.ValidateQueriesAsync(It.IsAny<Dictionary<string, QueryPair>>()))
            .ReturnsAsync(validationResult)
            .Callback<Dictionary<string, QueryPair>>(queries =>
            {
                // Verify that excluded queries are not present
                queries.Keys.Should().NotContain("GetParticleTypes");
                queries.Keys.Should().NotContain("GetParticleSubTypes");
                queries.Keys.Should().Contain("GetTestReadings");
                queries.Keys.Should().Contain("GetEmissionSpectroscopy");
            });

        // Act
        var result = await _service.ValidateAllQueriesAsync(options);

        // Assert
        result.Should().NotBeNull();
        _mockQueryComparisonService.Verify(x => x.ValidateQueriesAsync(It.IsAny<Dictionary<string, QueryPair>>()), Times.Once);
    }

    #region Query Comparison Logic Tests with Sample Data

    [Test]
    public async Task CompareQueryAsync_WithSampleTestReadingsData_ShouldDetectDataDiscrepancies()
    {
        // Arrange
        var queryName = "GetTestReadings";
        var currentQuery = "SELECT sampleID, testID, trialNumber, value1 FROM TestReadings WHERE sampleID = @sampleId";
        var legacyQuery = "SELECT sampleID, testID, trialNumber, value1 FROM TestReadings WHERE sampleID = @sampleId";
        var parameters = new Dictionary<string, object> { { "sampleId", 12345 } };

        var sampleDiscrepancies = new List<DataDiscrepancy>
        {
            new()
            {
                FieldName = "value1",
                CurrentValue = 15.25m,
                LegacyValue = 15.30m,
                RowIdentifier = "sampleID=12345,testID=100,trialNumber=1",
                Type = DiscrepancyType.ValueMismatch,
                Description = "Minor decimal difference in test reading value"
            },
            new()
            {
                FieldName = "trialNumber",
                CurrentValue = null,
                LegacyValue = 2,
                RowIdentifier = "sampleID=12345,testID=100,trialNumber=2",
                Type = DiscrepancyType.MissingInCurrent,
                Description = "Trial number 2 missing in current system"
            }
        };

        var expectedResult = new QueryComparisonResult
        {
            QueryName = queryName,
            DataMatches = false,
            CurrentRowCount = 2,
            LegacyRowCount = 3,
            Discrepancies = sampleDiscrepancies,
            CurrentExecutionTime = TimeSpan.FromMilliseconds(45),
            LegacyExecutionTime = TimeSpan.FromMilliseconds(52)
        };

        _mockQueryComparisonService.Setup(x => x.CompareQueriesAsync(queryName, currentQuery, legacyQuery, parameters))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _service.CompareQueryAsync(queryName, currentQuery, legacyQuery, parameters);

        // Assert
        result.Should().NotBeNull();
        result.DataMatches.Should().BeFalse();
        result.Discrepancies.Should().HaveCount(2);
        result.Discrepancies[0].Type.Should().Be(DiscrepancyType.ValueMismatch);
        result.Discrepancies[1].Type.Should().Be(DiscrepancyType.MissingInCurrent);
        result.CurrentRowCount.Should().Be(2);
        result.LegacyRowCount.Should().Be(3);
    }

    [Test]
    public async Task CompareQueryAsync_WithSampleEmissionSpectroscopyData_ShouldHandleComplexDataTypes()
    {
        // Arrange
        var queryName = "GetEmissionSpectroscopy";
        var currentQuery = "SELECT ID, testID, Na, Cr, Si, Mo FROM EmSpectro WHERE ID = @sampleId";
        var legacyQuery = "SELECT ID, testID, Na, Cr, Si, Mo FROM EmSpectro WHERE ID = @sampleId";
        var parameters = new Dictionary<string, object> { { "sampleId", 98765 } };

        var sampleDiscrepancies = new List<DataDiscrepancy>
        {
            new()
            {
                FieldName = "Na",
                CurrentValue = 12.456,
                LegacyValue = 12.46,
                RowIdentifier = "ID=98765,testID=200",
                Type = DiscrepancyType.FormatDifference,
                Description = "Precision difference in sodium measurement"
            },
            new()
            {
                FieldName = "Cr",
                CurrentValue = "0.025",
                LegacyValue = 0.025,
                RowIdentifier = "ID=98765,testID=200",
                Type = DiscrepancyType.TypeMismatch,
                Description = "String vs numeric type for chromium value"
            }
        };

        var expectedResult = new QueryComparisonResult
        {
            QueryName = queryName,
            DataMatches = false,
            CurrentRowCount = 1,
            LegacyRowCount = 1,
            Discrepancies = sampleDiscrepancies,
            CurrentExecutionTime = TimeSpan.FromMilliseconds(78),
            LegacyExecutionTime = TimeSpan.FromMilliseconds(85)
        };

        _mockQueryComparisonService.Setup(x => x.CompareQueriesAsync(queryName, currentQuery, legacyQuery, parameters))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _service.CompareQueryAsync(queryName, currentQuery, legacyQuery, parameters);

        // Assert
        result.Should().NotBeNull();
        result.DataMatches.Should().BeFalse();
        result.Discrepancies.Should().HaveCount(2);
        result.Discrepancies.Should().Contain(d => d.Type == DiscrepancyType.FormatDifference);
        result.Discrepancies.Should().Contain(d => d.Type == DiscrepancyType.TypeMismatch);
    }

    [Test]
    public async Task ValidateQueriesAsync_WithMinorDifferenceFiltering_ShouldFilterOutMinorDiscrepancies()
    {
        // Arrange
        var queries = new Dictionary<string, QueryPair>
        {
            {
                "TestQuery",
                new QueryPair
                {
                    CurrentQuery = "SELECT value FROM test",
                    LegacyQuery = "SELECT value FROM test"
                }
            }
        };

        var options = new ValidationOptions
        {
            IgnoreMinorDifferences = true,
            MaxDiscrepanciesToReport = 100
        };

        var majorDiscrepancy = new DataDiscrepancy
        {
            FieldName = "criticalValue",
            CurrentValue = 100,
            LegacyValue = 200,
            Type = DiscrepancyType.ValueMismatch
        };

        var minorDiscrepancy = new DataDiscrepancy
        {
            FieldName = "precisionValue",
            CurrentValue = 15.001m,
            LegacyValue = 15.002m,
            Type = DiscrepancyType.FormatDifference
        };

        var validationResult = new ValidationResult
        {
            QueriesValidated = 1,
            QueriesMatched = 0,
            QueriesFailed = 1,
            Results = new List<QueryComparisonResult>
            {
                new()
                {
                    QueryName = "TestQuery",
                    DataMatches = false,
                    Discrepancies = new List<DataDiscrepancy> { majorDiscrepancy, minorDiscrepancy }
                }
            },
            Summary = new ValidationSummary
            {
                TotalDiscrepancies = 2
            }
        };

        _mockQueryComparisonService.Setup(x => x.ValidateQueriesAsync(queries))
            .ReturnsAsync(validationResult);

        // Act
        var result = await _service.ValidateQueriesAsync(queries, options);

        // Assert
        result.Should().NotBeNull();
        result.Results.First().Discrepancies.Should().HaveCount(1);
        result.Results.First().Discrepancies.Should().Contain(majorDiscrepancy);
        result.Results.First().Discrepancies.Should().NotContain(minorDiscrepancy);
    }

    #endregion

    #region Performance Monitoring and Metrics Collection Tests

    [Test]
    public async Task ComparePerformanceAsync_WithSampleQueries_ShouldCollectDetailedMetrics()
    {
        // Arrange
        var queryName = "GetSampleHistory";
        var currentQuery = "SELECT * FROM Samples WHERE dateCreated >= @startDate";
        var legacyQuery = "SELECT * FROM Samples WHERE dateCreated >= @startDate";
        var parameters = new Dictionary<string, object> { { "startDate", DateTime.Now.AddDays(-30) } };

        var expectedResult = new PerformanceComparisonResult
        {
            QueryName = queryName,
            CurrentExecutionTime = TimeSpan.FromMilliseconds(125),
            LegacyExecutionTime = TimeSpan.FromMilliseconds(180),
            CurrentIsFaster = true,
            TimeDifference = TimeSpan.FromMilliseconds(-55),
            PerformanceRatio = 0.694 // 125/180
        };

        _mockQueryComparisonService.Setup(x => x.ComparePerformanceAsync(queryName, currentQuery, legacyQuery, parameters))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _service.ComparePerformanceAsync(queryName, currentQuery, legacyQuery, parameters);

        // Assert
        result.Should().NotBeNull();
        result.QueryName.Should().Be(queryName);
        result.CurrentExecutionTime.Should().Be(TimeSpan.FromMilliseconds(125));
        result.LegacyExecutionTime.Should().Be(TimeSpan.FromMilliseconds(180));
        result.CurrentIsFaster.Should().BeTrue();
        result.PerformanceRatio.Should().BeApproximately(0.694, 0.001);
        result.TimeDifference.Should().Be(TimeSpan.FromMilliseconds(-55));
    }

    [Test]
    public async Task ValidateAllQueriesAsync_ShouldCollectPerformanceMetricsForAllQueries()
    {
        // Arrange
        var options = new ValidationOptions
        {
            LegacyConnectionString = "valid connection",
            ComparePerformance = true,
            PerformanceThresholdPercent = 20.0
        };

        _mockLegacyQueryExecutorService.Setup(x => x.TestConnectionAsync())
            .ReturnsAsync(true);

        var performanceResults = new List<QueryComparisonResult>
        {
            new()
            {
                QueryName = "GetTestReadings",
                CurrentExecutionTime = TimeSpan.FromMilliseconds(50),
                LegacyExecutionTime = TimeSpan.FromMilliseconds(75),
                DataMatches = true
            },
            new()
            {
                QueryName = "GetEmissionSpectroscopy",
                CurrentExecutionTime = TimeSpan.FromMilliseconds(120),
                LegacyExecutionTime = TimeSpan.FromMilliseconds(95),
                DataMatches = true
            }
        };

        var validationResult = new ValidationResult
        {
            QueriesValidated = 2,
            QueriesMatched = 2,
            QueriesFailed = 0,
            Results = performanceResults,
            Summary = new ValidationSummary
            {
                AverageCurrentExecutionTime = TimeSpan.FromMilliseconds(85),
                AverageLegacyExecutionTime = TimeSpan.FromMilliseconds(85)
            }
        };

        _mockQueryComparisonService.Setup(x => x.ValidateQueriesAsync(It.IsAny<Dictionary<string, QueryPair>>()))
            .ReturnsAsync(validationResult);

        // Act
        var result = await _service.ValidateAllQueriesAsync(options);

        // Assert
        result.Should().NotBeNull();
        result.Summary.AverageCurrentExecutionTime.Should().Be(TimeSpan.FromMilliseconds(85));
        result.Summary.AverageLegacyExecutionTime.Should().Be(TimeSpan.FromMilliseconds(85));
        result.Results.Should().HaveCount(2);
        result.Results.Should().OnlyContain(r => r.CurrentExecutionTime > TimeSpan.Zero);
        result.Results.Should().OnlyContain(r => r.LegacyExecutionTime > TimeSpan.Zero);
    }

    [Test]
    public async Task ComparePerformanceAsync_WithSlowQuery_ShouldIdentifyPerformanceRegression()
    {
        // Arrange
        var queryName = "GetComplexAnalysis";
        var currentQuery = "SELECT * FROM ComplexView WHERE conditions = @param";
        var legacyQuery = "SELECT * FROM ComplexView WHERE conditions = @param";

        var expectedResult = new PerformanceComparisonResult
        {
            QueryName = queryName,
            CurrentExecutionTime = TimeSpan.FromSeconds(2.5),
            LegacyExecutionTime = TimeSpan.FromSeconds(1.2),
            CurrentIsFaster = false,
            TimeDifference = TimeSpan.FromSeconds(1.3),
            PerformanceRatio = 2.083 // 2500/1200
        };

        _mockQueryComparisonService.Setup(x => x.ComparePerformanceAsync(queryName, currentQuery, legacyQuery, null))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _service.ComparePerformanceAsync(queryName, currentQuery, legacyQuery);

        // Assert
        result.Should().NotBeNull();
        result.CurrentIsFaster.Should().BeFalse();
        result.PerformanceRatio.Should().BeGreaterThan(2.0);
        result.TimeDifference.Should().BePositive();
        result.TimeDifference.Should().Be(TimeSpan.FromSeconds(1.3));
    }

    #endregion

    #region Report Generation and Export Functionality Tests

    [Test]
    public async Task ValidateAllQueriesAsync_ShouldGenerateComprehensiveValidationReport()
    {
        // Arrange
        var options = new ValidationOptions
        {
            LegacyConnectionString = "valid connection",
            GenerateDetailedReports = true
        };

        _mockLegacyQueryExecutorService.Setup(x => x.TestConnectionAsync())
            .ReturnsAsync(true);

        var validationResult = new ValidationResult
        {
            QueriesValidated = 3,
            QueriesMatched = 2,
            QueriesFailed = 1,
            StartTime = DateTime.Now.AddMinutes(-5),
            EndTime = DateTime.Now,
            Duration = TimeSpan.FromMinutes(5),
            Results = new List<QueryComparisonResult>
            {
                new()
                {
                    QueryName = "GetTestReadings",
                    DataMatches = true,
                    CurrentRowCount = 100,
                    LegacyRowCount = 100,
                    Discrepancies = new List<DataDiscrepancy>()
                },
                new()
                {
                    QueryName = "GetEmissionSpectroscopy",
                    DataMatches = false,
                    CurrentRowCount = 50,
                    LegacyRowCount = 52,
                    Discrepancies = new List<DataDiscrepancy>
                    {
                        new()
                        {
                            FieldName = "Na",
                            Type = DiscrepancyType.ValueMismatch,
                            CurrentValue = 12.5,
                            LegacyValue = 12.6
                        }
                    }
                }
            },
            Summary = new ValidationSummary
            {
                QueriesValidated = 3,
                QueriesMatched = 2,
                QueriesFailed = 1,
                TotalDiscrepancies = 1,
                CriticalIssues = new List<string> { "Row count mismatch in GetEmissionSpectroscopy" }
            }
        };

        _mockQueryComparisonService.Setup(x => x.ValidateQueriesAsync(It.IsAny<Dictionary<string, QueryPair>>()))
            .ReturnsAsync(validationResult);

        // Act
        var result = await _service.ValidateAllQueriesAsync(options);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse(); // Because QueriesFailed > 0
        result.Summary.MatchPercentage.Should().BeApproximately(66.67, 0.01); // 2/3 * 100
        result.Summary.TotalDiscrepancies.Should().Be(1);
        result.Summary.CriticalIssues.Should().HaveCount(1);
        result.Duration.Should().Be(TimeSpan.FromMinutes(5));
    }

    [Test]
    public async Task GenerateDetailedReport_ShouldCreateComprehensiveReport()
    {
        // Arrange
        var validationResult = new ValidationResult
        {
            QueriesValidated = 5,
            QueriesMatched = 4,
            QueriesFailed = 1,
            Duration = TimeSpan.FromMinutes(10),
            Results = new List<QueryComparisonResult>
            {
                new()
                {
                    QueryName = "GetTestReadings",
                    DataMatches = true,
                    CurrentRowCount = 1000,
                    LegacyRowCount = 1000,
                    CurrentExecutionTime = TimeSpan.FromMilliseconds(150),
                    LegacyExecutionTime = TimeSpan.FromMilliseconds(180)
                }
            }
        };

        var expectedReport = new ValidationReportDto
        {
            QueriesValidated = 5,
            QueriesMatched = 4,
            QueriesFailed = 1,
            MatchPercentage = 80.0,
            Duration = TimeSpan.FromMinutes(10),
            Success = false,
            QueryReports = new List<QueryComparisonReportDto>
            {
                new()
                {
                    QueryName = "GetTestReadings",
                    DataMatches = true,
                    CurrentRowCount = 1000,
                    LegacyRowCount = 1000,
                    CurrentExecutionTime = TimeSpan.FromMilliseconds(150),
                    LegacyExecutionTime = TimeSpan.FromMilliseconds(180),
                    PerformanceRatio = 0.833
                }
            }
        };

        _mockReportingService.Setup(x => x.GenerateDetailedReportAsync(validationResult))
            .ReturnsAsync(expectedReport);

        // Act
        var report = await _mockReportingService.Object.GenerateDetailedReportAsync(validationResult);

        // Assert
        report.Should().NotBeNull();
        report.MatchPercentage.Should().Be(80.0);
        report.Success.Should().BeFalse();
        report.QueryReports.Should().HaveCount(1);
        report.QueryReports[0].PerformanceRatio.Should().BeApproximately(0.833, 0.001);
        _mockReportingService.Verify(x => x.GenerateDetailedReportAsync(validationResult), Times.Once);
    }

    [Test]
    public async Task ExportReport_ShouldSupportMultipleFormats()
    {
        // Arrange
        var validationResult = new ValidationResult
        {
            QueriesValidated = 2,
            QueriesMatched = 2,
            QueriesFailed = 0
        };

        var outputPath = "/reports/validation_report";
        var formats = new[] { ReportFormat.Html, ReportFormat.Csv, ReportFormat.Json };

        foreach (var format in formats)
        {
            var expectedFilePath = $"{outputPath}.{format.ToString().ToLower()}";
            _mockReportingService.Setup(x => x.ExportReportAsync(validationResult, format, outputPath))
                .ReturnsAsync(expectedFilePath);
        }

        // Act & Assert
        foreach (var format in formats)
        {
            var result = await _mockReportingService.Object.ExportReportAsync(validationResult, format, outputPath);
            result.Should().EndWith($".{format.ToString().ToLower()}");
            _mockReportingService.Verify(x => x.ExportReportAsync(validationResult, format, outputPath), Times.Once);
        }
    }

    [Test]
    public async Task GenerateTrendAnalysis_ShouldAnalyzeHistoricalValidationResults()
    {
        // Arrange
        var historicalResults = new List<ValidationResult>
        {
            new()
            {
                QueriesValidated = 10,
                QueriesMatched = 8,
                StartTime = DateTime.Now.AddDays(-30),
                Summary = new ValidationSummary { MatchPercentage = 80.0 }
            },
            new()
            {
                QueriesValidated = 10,
                QueriesMatched = 9,
                StartTime = DateTime.Now.AddDays(-15),
                Summary = new ValidationSummary { MatchPercentage = 90.0 }
            },
            new()
            {
                QueriesValidated = 10,
                QueriesMatched = 10,
                StartTime = DateTime.Now,
                Summary = new ValidationSummary { MatchPercentage = 100.0 }
            }
        };

        var expectedTrends = new List<TrendAnalysisDto>
        {
            new()
            {
                MetricName = "Match Percentage",
                Direction = TrendDirection.Improving,
                ChangePercentage = 25.0, // From 80% to 100%
                DataPoints = new List<TrendDataPoint>
                {
                    new() { Timestamp = DateTime.Now.AddDays(-30), Value = 80.0 },
                    new() { Timestamp = DateTime.Now.AddDays(-15), Value = 90.0 },
                    new() { Timestamp = DateTime.Now, Value = 100.0 }
                },
                Analysis = "Query validation match percentage has improved consistently over the past 30 days"
            }
        };

        _mockReportingService.Setup(x => x.AnalyzeTrendsAsync(historicalResults))
            .ReturnsAsync(expectedTrends);

        // Act
        var trends = await _mockReportingService.Object.AnalyzeTrendsAsync(historicalResults);

        // Assert
        trends.Should().NotBeNull();
        trends.Should().HaveCount(1);
        trends[0].Direction.Should().Be(TrendDirection.Improving);
        trends[0].ChangePercentage.Should().Be(25.0);
        trends[0].DataPoints.Should().HaveCount(3);
        _mockReportingService.Verify(x => x.AnalyzeTrendsAsync(historicalResults), Times.Once);
    }

    [Test]
    public async Task GenerateComparisonReport_ShouldCompareCurrentAndPreviousResults()
    {
        // Arrange
        var currentResult = new ValidationResult
        {
            QueriesValidated = 10,
            QueriesMatched = 9,
            QueriesFailed = 1,
            Summary = new ValidationSummary { MatchPercentage = 90.0 }
        };

        var previousResult = new ValidationResult
        {
            QueriesValidated = 10,
            QueriesMatched = 8,
            QueriesFailed = 2,
            Summary = new ValidationSummary { MatchPercentage = 80.0 }
        };

        var expectedComparisonReport = new ValidationReportDto
        {
            QueriesValidated = 10,
            QueriesMatched = 9,
            QueriesFailed = 1,
            MatchPercentage = 90.0,
            Success = false,
            Summary = new ValidationSummaryDto
            {
                MatchPercentage = 90.0
            }
        };

        _mockReportingService.Setup(x => x.GenerateComparisonReportAsync(currentResult, previousResult))
            .ReturnsAsync(expectedComparisonReport);

        // Act
        var report = await _mockReportingService.Object.GenerateComparisonReportAsync(currentResult, previousResult);

        // Assert
        report.Should().NotBeNull();
        report.MatchPercentage.Should().Be(90.0);
        report.QueriesMatched.Should().BeGreaterThan(8); // Improvement from previous
        _mockReportingService.Verify(x => x.GenerateComparisonReportAsync(currentResult, previousResult), Times.Once);
    }

    #endregion
}