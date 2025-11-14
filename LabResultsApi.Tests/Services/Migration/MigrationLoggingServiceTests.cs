using NUnit.Framework;
using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using LabResultsApi.Services.Migration;
using LabResultsApi.Models.Migration;
using FluentAssertions;

namespace LabResultsApi.Tests.Services.Migration;

[TestFixture]
public class MigrationLoggingServiceTests
{
    private Mock<ILogger<MigrationLoggingService>> _mockLogger;
    private Mock<IConfiguration> _mockConfiguration;
    private MigrationLoggingService _service;
    private string _testLogDirectory;

    [SetUp]
    public void SetUp()
    {
        _mockLogger = new Mock<ILogger<MigrationLoggingService>>();
        _mockConfiguration = new Mock<IConfiguration>();
        
        // Create a temporary directory for test logs
        _testLogDirectory = Path.Combine(Path.GetTempPath(), $"migration-test-logs-{Guid.NewGuid()}");
        Directory.CreateDirectory(_testLogDirectory);
        
        _mockConfiguration.Setup(c => c.GetValue<string>("Migration:LogDirectory"))
            .Returns(_testLogDirectory);

        _service = new MigrationLoggingService(_mockLogger.Object, _mockConfiguration.Object);
    }

    [TearDown]
    public void TearDown()
    {
        // Clean up test log directory
        if (Directory.Exists(_testLogDirectory))
        {
            Directory.Delete(_testLogDirectory, true);
        }
    }

    [Test]
    public async Task LogMigrationStartAsync_ShouldCreateLogEntry()
    {
        // Arrange
        var migrationId = Guid.NewGuid();
        var options = new MigrationOptions { ClearExistingData = true };

        // Act
        await _service.LogMigrationStartAsync(migrationId, options);

        // Assert
        var logs = await _service.GetMigrationLogsAsync(migrationId);
        logs.Should().HaveCount(1);
        logs[0].Level.Should().Be(ErrorLevel.Info);
        logs[0].Component.Should().Be("MigrationControl");
        logs[0].Message.Should().Be("Migration started");
        logs[0].Details.Should().NotBeNullOrEmpty();
    }

    [Test]
    public async Task LogMigrationEndAsync_WithCompletedStatus_ShouldCreateInfoLogEntry()
    {
        // Arrange
        var migrationId = Guid.NewGuid();
        var duration = TimeSpan.FromMinutes(5);

        // Act
        await _service.LogMigrationEndAsync(migrationId, MigrationStatus.Completed, duration);

        // Assert
        var logs = await _service.GetMigrationLogsAsync(migrationId);
        logs.Should().HaveCount(1);
        logs[0].Level.Should().Be(ErrorLevel.Info);
        logs[0].Message.Should().Contain("Completed");
        logs[0].Details.Should().Contain("Duration");
    }

    [Test]
    public async Task LogMigrationEndAsync_WithFailedStatus_ShouldCreateErrorLogEntry()
    {
        // Arrange
        var migrationId = Guid.NewGuid();
        var duration = TimeSpan.FromMinutes(2);

        // Act
        await _service.LogMigrationEndAsync(migrationId, MigrationStatus.Failed, duration);

        // Assert
        var logs = await _service.GetMigrationLogsAsync(migrationId);
        logs.Should().HaveCount(1);
        logs[0].Level.Should().Be(ErrorLevel.Error);
        logs[0].Message.Should().Contain("Failed");
    }

    [Test]
    public async Task LogProgressAsync_ShouldCreateProgressLogEntry()
    {
        // Arrange
        var migrationId = Guid.NewGuid();
        var progressPercentage = 50.0;
        var operation = "Processing table data";

        // Act
        await _service.LogProgressAsync(migrationId, progressPercentage, operation);

        // Assert
        var logs = await _service.GetMigrationLogsAsync(migrationId);
        logs.Should().HaveCount(1);
        logs[0].Level.Should().Be(ErrorLevel.Info);
        logs[0].Message.Should().Contain("50.0%");
        logs[0].Details.Should().Be(operation);
    }

    [Test]
    public async Task LogErrorAsync_ShouldCreateErrorLogEntry()
    {
        // Arrange
        var migrationId = Guid.NewGuid();
        var error = new MigrationError
        {
            Level = ErrorLevel.Error,
            Component = "TestComponent",
            Message = "Test error message",
            Details = "Error details",
            TableName = "TestTable",
            RecordNumber = 123
        };

        // Act
        await _service.LogErrorAsync(migrationId, error);

        // Assert
        var logs = await _service.GetMigrationLogsAsync(migrationId);
        logs.Should().HaveCount(1);
        logs[0].Should().BeEquivalentTo(error);
    }

    [Test]
    public async Task LogPhaseStartAsync_ShouldCreatePhaseStartLogEntry()
    {
        // Arrange
        var migrationId = Guid.NewGuid();
        var phaseName = "DatabaseSeeding";

        // Act
        await _service.LogPhaseStartAsync(migrationId, phaseName);

        // Assert
        var logs = await _service.GetMigrationLogsAsync(migrationId);
        logs.Should().HaveCount(1);
        logs[0].Level.Should().Be(ErrorLevel.Info);
        logs[0].Component.Should().Be(phaseName);
        logs[0].Message.Should().Contain("Phase started");
    }

    [Test]
    public async Task LogPhaseEndAsync_WithSuccess_ShouldCreateSuccessLogEntry()
    {
        // Arrange
        var migrationId = Guid.NewGuid();
        var phaseName = "DatabaseSeeding";
        var duration = TimeSpan.FromMinutes(3);

        // Act
        await _service.LogPhaseEndAsync(migrationId, phaseName, true, duration);

        // Assert
        var logs = await _service.GetMigrationLogsAsync(migrationId);
        logs.Should().HaveCount(1);
        logs[0].Level.Should().Be(ErrorLevel.Info);
        logs[0].Message.Should().Contain("Success");
        logs[0].Details.Should().Contain("Duration");
    }

    [Test]
    public async Task LogPhaseEndAsync_WithFailure_ShouldCreateErrorLogEntry()
    {
        // Arrange
        var migrationId = Guid.NewGuid();
        var phaseName = "DatabaseSeeding";
        var duration = TimeSpan.FromMinutes(1);

        // Act
        await _service.LogPhaseEndAsync(migrationId, phaseName, false, duration);

        // Assert
        var logs = await _service.GetMigrationLogsAsync(migrationId);
        logs.Should().HaveCount(1);
        logs[0].Level.Should().Be(ErrorLevel.Error);
        logs[0].Message.Should().Contain("Failed");
    }

    [Test]
    public async Task GetMigrationLogsAsync_WithNonExistentMigration_ShouldReturnEmptyList()
    {
        // Arrange
        var migrationId = Guid.NewGuid();

        // Act
        var logs = await _service.GetMigrationLogsAsync(migrationId);

        // Assert
        logs.Should().BeEmpty();
    }

    [Test]
    public async Task GenerateLogReportAsync_ShouldCreateJsonReport()
    {
        // Arrange
        var migrationId = Guid.NewGuid();
        await _service.LogMigrationStartAsync(migrationId, new MigrationOptions());
        await _service.LogErrorAsync(migrationId, new MigrationError 
        { 
            Level = ErrorLevel.Error, 
            Message = "Test error" 
        });

        // Act
        var report = await _service.GenerateLogReportAsync(migrationId);

        // Assert
        report.Should().NotBeNullOrEmpty();
        report.Should().Contain("MigrationId");
        report.Should().Contain("TotalEntries");
        report.Should().Contain("ErrorCount");
        report.Should().Contain(migrationId.ToString());
    }

    [Test]
    public async Task LogMigrationStartAsync_ShouldCreateLogFile()
    {
        // Arrange
        var migrationId = Guid.NewGuid();
        var options = new MigrationOptions();

        // Act
        await _service.LogMigrationStartAsync(migrationId, options);

        // Assert
        var logFileName = $"migration-{migrationId}.log";
        var logFilePath = Path.Combine(_testLogDirectory, logFileName);
        File.Exists(logFilePath).Should().BeTrue();
        
        var logContent = await File.ReadAllTextAsync(logFilePath);
        logContent.Should().Contain("Migration started");
    }

    [Test]
    public async Task ArchiveLogsAsync_ShouldMoveOldLogFiles()
    {
        // Arrange
        var migrationId = Guid.NewGuid();
        await _service.LogMigrationStartAsync(migrationId, new MigrationOptions());
        
        var logFileName = $"migration-{migrationId}.log";
        var logFilePath = Path.Combine(_testLogDirectory, logFileName);
        
        // Make the file appear old by setting creation time
        File.SetCreationTime(logFilePath, DateTime.Now.AddDays(-2));
        
        var archiveThreshold = DateTime.Now.AddDays(-1);

        // Act
        await _service.ArchiveLogsAsync(archiveThreshold);

        // Assert
        var archiveDirectory = Path.Combine(_testLogDirectory, "archive");
        var archivedFilePath = Path.Combine(archiveDirectory, logFileName);
        
        File.Exists(logFilePath).Should().BeFalse();
        File.Exists(archivedFilePath).Should().BeTrue();
    }

    [Test]
    public async Task LogProgressAsync_WithMultipleOf10Percent_ShouldWriteToFile()
    {
        // Arrange
        var migrationId = Guid.NewGuid();

        // Act
        await _service.LogProgressAsync(migrationId, 20.0, "Test operation");

        // Assert
        var logFileName = $"migration-{migrationId}.log";
        var logFilePath = Path.Combine(_testLogDirectory, logFileName);
        File.Exists(logFilePath).Should().BeTrue();
        
        var logContent = await File.ReadAllTextAsync(logFilePath);
        logContent.Should().Contain("20.0%");
    }

    [Test]
    public async Task LogProgressAsync_WithNonMultipleOf10Percent_ShouldNotWriteToFile()
    {
        // Arrange
        var migrationId = Guid.NewGuid();

        // Act
        await _service.LogProgressAsync(migrationId, 15.0, "Test operation");

        // Assert
        var logFileName = $"migration-{migrationId}.log";
        var logFilePath = Path.Combine(_testLogDirectory, logFileName);
        File.Exists(logFilePath).Should().BeFalse();
    }
}