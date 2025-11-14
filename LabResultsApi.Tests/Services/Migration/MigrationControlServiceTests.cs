using NUnit.Framework;
using Moq;
using Microsoft.Extensions.Logging;
using LabResultsApi.Services.Migration;
using LabResultsApi.Models.Migration;
using FluentAssertions;

namespace LabResultsApi.Tests.Services.Migration;

[TestFixture]
public class MigrationControlServiceTests
{
    private Mock<ILogger<MigrationControlService>> _mockLogger;
    private Mock<IDatabaseSeedingService> _mockSeedingService;
    private Mock<ISqlValidationService> _mockValidationService;
    private Mock<ISsoMigrationService> _mockSsoMigrationService;
    private MigrationControlService _service;

    [SetUp]
    public void SetUp()
    {
        _mockLogger = new Mock<ILogger<MigrationControlService>>();
        _mockSeedingService = new Mock<IDatabaseSeedingService>();
        _mockValidationService = new Mock<ISqlValidationService>();
        _mockSsoMigrationService = new Mock<ISsoMigrationService>();

        _service = new MigrationControlService(
            _mockLogger.Object,
            _mockSeedingService.Object,
            _mockValidationService.Object,
            _mockSsoMigrationService.Object);
    }

    [TearDown]
    public void TearDown()
    {
        _service?.Dispose();
    }

    [Test]
    public async Task ExecuteFullMigrationAsync_WithDefaultOptions_ShouldCompleteSuccessfully()
    {
        // Arrange
        var options = new MigrationOptions
        {
            ClearExistingData = true,
            CreateMissingTables = true,
            ValidateAgainstLegacy = false,
            RemoveAuthentication = false
        };

        // Act
        var result = await _service.ExecuteFullMigrationAsync(options);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be(MigrationStatus.Completed);
        result.StartTime.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
        result.EndTime.Should().NotBeNull();
        result.SeedingResult.Should().NotBeNull();
        result.ValidationResult.Should().BeNull(); // Not enabled
        result.AuthRemovalResult.Should().BeNull(); // Not enabled
    }

    [Test]
    public async Task ExecuteFullMigrationAsync_WithValidationEnabled_ShouldIncludeValidationResult()
    {
        // Arrange
        var options = new MigrationOptions
        {
            ValidateAgainstLegacy = true,
            RemoveAuthentication = false
        };

        // Act
        var result = await _service.ExecuteFullMigrationAsync(options);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be(MigrationStatus.Completed);
        result.SeedingResult.Should().NotBeNull();
        result.ValidationResult.Should().NotBeNull();
        result.AuthRemovalResult.Should().BeNull(); // Not enabled
    }

    [Test]
    public async Task ExecuteFullMigrationAsync_WithAuthRemovalEnabled_ShouldIncludeAuthRemovalResult()
    {
        // Arrange
        var options = new MigrationOptions
        {
            ValidateAgainstLegacy = false,
            RemoveAuthentication = true
        };

        // Act
        var result = await _service.ExecuteFullMigrationAsync(options);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be(MigrationStatus.Completed);
        result.SeedingResult.Should().NotBeNull();
        result.ValidationResult.Should().BeNull(); // Not enabled
        result.AuthRemovalResult.Should().NotBeNull();
    }

    [Test]
    public async Task ExecuteFullMigrationAsync_WhenMigrationAlreadyRunning_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var options = new MigrationOptions();
        
        // Start first migration
        var firstMigrationTask = _service.ExecuteFullMigrationAsync(options);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.ExecuteFullMigrationAsync(options));
        
        exception.Message.Should().Contain("migration is already in progress");

        // Wait for first migration to complete
        await firstMigrationTask;
    }

    [Test]
    public async Task ExecuteFullMigrationAsync_WhenCancelled_ShouldReturnCancelledStatus()
    {
        // Arrange
        var options = new MigrationOptions();
        var cancellationTokenSource = new CancellationTokenSource();

        // Act
        var migrationTask = _service.ExecuteFullMigrationAsync(options, cancellationTokenSource.Token);
        cancellationTokenSource.Cancel();
        var result = await migrationTask;

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be(MigrationStatus.Cancelled);
    }

    [Test]
    public async Task GetMigrationStatusAsync_WhenNoMigrationRunning_ShouldReturnNotStarted()
    {
        // Act
        var status = await _service.GetMigrationStatusAsync();

        // Assert
        status.Should().Be(MigrationStatus.NotStarted);
    }

    [Test]
    public async Task GetMigrationStatusAsync_WhenMigrationRunning_ShouldReturnInProgress()
    {
        // Arrange
        var options = new MigrationOptions();
        var migrationTask = _service.ExecuteFullMigrationAsync(options);

        // Act
        var status = await _service.GetMigrationStatusAsync();

        // Assert
        status.Should().Be(MigrationStatus.InProgress);

        // Wait for migration to complete
        await migrationTask;
    }

    [Test]
    public async Task GetCurrentMigrationAsync_WhenNoMigrationRunning_ShouldReturnNull()
    {
        // Act
        var migration = await _service.GetCurrentMigrationAsync();

        // Assert
        migration.Should().BeNull();
    }

    [Test]
    public async Task GetCurrentMigrationAsync_WhenMigrationRunning_ShouldReturnCurrentMigration()
    {
        // Arrange
        var options = new MigrationOptions();
        var migrationTask = _service.ExecuteFullMigrationAsync(options);

        // Act
        var migration = await _service.GetCurrentMigrationAsync();

        // Assert
        migration.Should().NotBeNull();
        migration!.Status.Should().Be(MigrationStatus.InProgress);

        // Wait for migration to complete
        await migrationTask;
    }

    [Test]
    public async Task CancelMigrationAsync_WhenNoMigrationRunning_ShouldReturnFalse()
    {
        // Act
        var cancelled = await _service.CancelMigrationAsync();

        // Assert
        cancelled.Should().BeFalse();
    }

    [Test]
    public async Task CancelMigrationAsync_WhenMigrationRunning_ShouldReturnTrue()
    {
        // Arrange
        var options = new MigrationOptions();
        var migrationTask = _service.ExecuteFullMigrationAsync(options);

        // Act
        var cancelled = await _service.CancelMigrationAsync();

        // Assert
        cancelled.Should().BeTrue();

        // Wait for migration to complete
        var result = await migrationTask;
        result.Status.Should().Be(MigrationStatus.Cancelled);
    }

    [Test]
    public async Task IsMigrationRunningAsync_WhenNoMigrationRunning_ShouldReturnFalse()
    {
        // Act
        var isRunning = await _service.IsMigrationRunningAsync();

        // Assert
        isRunning.Should().BeFalse();
    }

    [Test]
    public async Task IsMigrationRunningAsync_WhenMigrationRunning_ShouldReturnTrue()
    {
        // Arrange
        var options = new MigrationOptions();
        var migrationTask = _service.ExecuteFullMigrationAsync(options);

        // Act
        var isRunning = await _service.IsMigrationRunningAsync();

        // Assert
        isRunning.Should().BeTrue();

        // Wait for migration to complete
        await migrationTask;
    }

    [Test]
    public async Task GetMigrationResultAsync_WithValidId_ShouldReturnResult()
    {
        // Arrange
        var options = new MigrationOptions();
        var result = await _service.ExecuteFullMigrationAsync(options);

        // Act
        var retrievedResult = await _service.GetMigrationResultAsync(result.MigrationId);

        // Assert
        retrievedResult.Should().NotBeNull();
        retrievedResult!.MigrationId.Should().Be(result.MigrationId);
        retrievedResult.Status.Should().Be(result.Status);
    }

    [Test]
    public async Task GetMigrationResultAsync_WithInvalidId_ShouldReturnNull()
    {
        // Act
        var result = await _service.GetMigrationResultAsync(Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    [Test]
    public async Task GetMigrationHistoryAsync_WithMultipleMigrations_ShouldReturnOrderedHistory()
    {
        // Arrange
        var options = new MigrationOptions();
        var result1 = await _service.ExecuteFullMigrationAsync(options);
        await Task.Delay(100); // Ensure different timestamps
        var result2 = await _service.ExecuteFullMigrationAsync(options);

        // Act
        var history = await _service.GetMigrationHistoryAsync();

        // Assert
        history.Should().HaveCount(2);
        history[0].MigrationId.Should().Be(result2.MigrationId); // Most recent first
        history[1].MigrationId.Should().Be(result1.MigrationId);
    }

    [Test]
    public async Task GetMigrationHistoryAsync_WithLimit_ShouldRespectLimit()
    {
        // Arrange
        var options = new MigrationOptions();
        await _service.ExecuteFullMigrationAsync(options);
        await _service.ExecuteFullMigrationAsync(options);
        await _service.ExecuteFullMigrationAsync(options);

        // Act
        var history = await _service.GetMigrationHistoryAsync(2);

        // Assert
        history.Should().HaveCount(2);
    }

    [Test]
    public void ProgressUpdated_Event_ShouldBeRaisedDuringMigration()
    {
        // Arrange
        var progressEvents = new List<MigrationProgressEventArgs>();
        _service.ProgressUpdated += (sender, args) => progressEvents.Add(args);
        var options = new MigrationOptions();

        // Act
        var migrationTask = _service.ExecuteFullMigrationAsync(options);
        migrationTask.Wait();

        // Assert
        progressEvents.Should().NotBeEmpty();
        progressEvents.Should().Contain(e => e.ProgressPercentage == 100);
    }

    [Test]
    public void StatusChanged_Event_ShouldBeRaisedDuringMigration()
    {
        // Arrange
        var statusEvents = new List<MigrationStatusEventArgs>();
        _service.StatusChanged += (sender, args) => statusEvents.Add(args);
        var options = new MigrationOptions();

        // Act
        var migrationTask = _service.ExecuteFullMigrationAsync(options);
        migrationTask.Wait();

        // Assert
        statusEvents.Should().NotBeEmpty();
        statusEvents.Should().Contain(e => e.NewStatus == MigrationStatus.InProgress);
        statusEvents.Should().Contain(e => e.NewStatus == MigrationStatus.Completed);
    }

    [Test]
    public async Task ExecuteFullMigrationAsync_ShouldTrackStatistics()
    {
        // Arrange
        var options = new MigrationOptions();

        // Act
        var result = await _service.ExecuteFullMigrationAsync(options);

        // Assert
        result.Statistics.Should().NotBeNull();
        result.Statistics.ProgressPercentage.Should().Be(100);
        result.Duration.Should().NotBeNull();
        result.Duration.Should().BeGreaterThan(TimeSpan.Zero);
    }
}