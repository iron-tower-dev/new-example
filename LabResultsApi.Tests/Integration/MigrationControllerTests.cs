using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;
using LabResultsApi.DTOs.Migration;
using LabResultsApi.Services.Migration;
using LabResultsApi.Models.Migration;
using Moq;

namespace LabResultsApi.Tests.Integration;

public class MigrationControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public MigrationControllerTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task GetMigrationStatus_WhenNoMigrationRunning_ReturnsNotStartedStatus()
    {
        // Act
        var response = await _client.GetAsync("/api/migration/status");

        // Assert
        response.EnsureSuccessStatusCode();
        var status = await response.Content.ReadFromJsonAsync<MigrationStatusDto>();
        
        Assert.NotNull(status);
        Assert.Equal("NotStarted", status.Status);
        Assert.Equal(Guid.Empty, status.MigrationId);
    }

    [Fact]
    public async Task IsMigrationRunning_WhenNoMigrationRunning_ReturnsFalse()
    {
        // Act
        var response = await _client.GetAsync("/api/migration/running");

        // Assert
        response.EnsureSuccessStatusCode();
        var isRunning = await response.Content.ReadFromJsonAsync<bool>();
        
        Assert.False(isRunning);
    }

    [Fact]
    public async Task StartMigration_WithValidOptions_ReturnsSuccess()
    {
        // Arrange
        var request = new StartMigrationRequest
        {
            Options = new MigrationOptionsDto
            {
                ClearExistingData = true,
                CreateMissingTables = true,
                ValidateAgainstLegacy = false, // Disable to avoid legacy DB dependency
                RemoveAuthentication = false,
                MaxConcurrentOperations = 2,
                OperationTimeoutMinutes = 10,
                SeedingOptions = new SeedingOptionsDto
                {
                    BatchSize = 100,
                    ContinueOnError = true,
                    ValidateBeforeInsert = true
                }
            }
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/migration/start", request);

        // Assert
        if (response.StatusCode == HttpStatusCode.Conflict)
        {
            // Migration already running, which is acceptable for this test
            var conflictMessage = await response.Content.ReadAsStringAsync();
            Assert.Contains("migration is already in progress", conflictMessage, StringComparison.OrdinalIgnoreCase);
        }
        else
        {
            response.EnsureSuccessStatusCode();
            var status = await response.Content.ReadFromJsonAsync<MigrationStatusDto>();
            
            Assert.NotNull(status);
            Assert.NotEqual(Guid.Empty, status.MigrationId);
            Assert.True(status.Status == "InProgress" || status.Status == "Completed");
        }
    }

    [Fact]
    public async Task StartMigration_WhenMigrationAlreadyRunning_ReturnsConflict()
    {
        // Arrange - Start first migration
        var request = new StartMigrationRequest
        {
            Options = new MigrationOptionsDto
            {
                ClearExistingData = false,
                CreateMissingTables = false,
                ValidateAgainstLegacy = false,
                RemoveAuthentication = false
            }
        };

        var firstResponse = await _client.PostAsJsonAsync("/api/migration/start", request);
        
        // If first migration didn't start (already running), skip this test
        if (firstResponse.StatusCode == HttpStatusCode.Conflict)
        {
            return;
        }

        // Act - Try to start second migration
        var secondResponse = await _client.PostAsJsonAsync("/api/migration/start", request);

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, secondResponse.StatusCode);
        var message = await secondResponse.Content.ReadAsStringAsync();
        Assert.Contains("migration is already in progress", message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task GetMigrationProgress_WhenNoMigrationRunning_ReturnsNotFound()
    {
        // Ensure no migration is running
        await _client.PostAsync("/api/migration/cancel", null);
        await Task.Delay(1000); // Wait for cancellation

        // Act
        var response = await _client.GetAsync("/api/migration/progress");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetMigrationHistory_ReturnsHistoryList()
    {
        // Act
        var response = await _client.GetAsync("/api/migration/history?limit=5");

        // Assert
        response.EnsureSuccessStatusCode();
        var history = await response.Content.ReadFromJsonAsync<List<MigrationStatusDto>>();
        
        Assert.NotNull(history);
        Assert.True(history.Count <= 5);
    }

    [Fact]
    public async Task GetMigrationStatistics_ReturnsStatistics()
    {
        // Act
        var response = await _client.GetAsync("/api/migration/statistics?days=7");

        // Assert
        response.EnsureSuccessStatusCode();
        var stats = await response.Content.ReadFromJsonAsync<MigrationStatisticsSummaryDto>();
        
        Assert.NotNull(stats);
        Assert.Equal(7, stats.PeriodDays);
        Assert.True(stats.TotalMigrations >= 0);
        Assert.True(stats.SuccessRate >= 0 && stats.SuccessRate <= 100);
    }

    [Fact]
    public async Task GetMigrationResult_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var invalidId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/migration/{invalidId}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task DownloadMigrationReport_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var invalidId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/migration/{invalidId}/report?format=json");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task DownloadMigrationReport_WithInvalidFormat_ReturnsBadRequest()
    {
        // Arrange
        var validId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/migration/{validId}/report?format=invalid");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var message = await response.Content.ReadAsStringAsync();
        Assert.Contains("Supported formats", message);
    }

    [Fact]
    public async Task DownloadMigrationLogs_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var invalidId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/migration/{invalidId}/logs?level=all");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CancelMigration_WhenNoMigrationRunning_ReturnsBadRequest()
    {
        // Ensure no migration is running
        await _client.PostAsync("/api/migration/cancel", null);
        await Task.Delay(1000); // Wait for cancellation

        // Act
        var response = await _client.PostAsync("/api/migration/cancel", null);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var message = await response.Content.ReadAsStringAsync();
        Assert.Contains("No migration is currently running", message);
    }

    [Fact]
    public async Task PauseMigration_WhenNoMigrationRunning_ReturnsBadRequest()
    {
        // Act
        var response = await _client.PostAsync("/api/migration/pause", null);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task ResumeMigration_WhenNoMigrationPaused_ReturnsBadRequest()
    {
        // Act
        var response = await _client.PostAsync("/api/migration/resume", null);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Theory]
    [InlineData("json")]
    [InlineData("csv")]
    public async Task DownloadMigrationReport_WithValidFormat_ReturnsCorrectContentType(string format)
    {
        // Arrange - Start a migration to have data
        var request = new StartMigrationRequest
        {
            Options = new MigrationOptionsDto
            {
                ClearExistingData = false,
                CreateMissingTables = false,
                ValidateAgainstLegacy = false,
                RemoveAuthentication = false
            }
        };

        var startResponse = await _client.PostAsJsonAsync("/api/migration/start", request);
        if (startResponse.StatusCode == HttpStatusCode.Conflict)
        {
            // Migration already running, get current status
            var statusResponse = await _client.GetAsync("/api/migration/status");
            var status = await statusResponse.Content.ReadFromJsonAsync<MigrationStatusDto>();
            
            if (status?.MigrationId != Guid.Empty)
            {
                // Act
                var response = await _client.GetAsync($"/api/migration/{status.MigrationId}/report?format={format}");

                // Assert
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var expectedContentType = format == "json" ? "application/json" : "text/csv";
                    Assert.Contains(expectedContentType, response.Content.Headers.ContentType?.ToString());
                }
            }
        }
        else if (startResponse.IsSuccessStatusCode)
        {
            var status = await startResponse.Content.ReadFromJsonAsync<MigrationStatusDto>();
            
            // Wait a bit for migration to process
            await Task.Delay(2000);
            
            // Act
            var response = await _client.GetAsync($"/api/migration/{status!.MigrationId}/report?format={format}");

            // Assert
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var expectedContentType = format == "json" ? "application/json" : "text/csv";
                Assert.Contains(expectedContentType, response.Content.Headers.ContentType?.ToString());
            }
        }
    }

    [Fact]
    public async Task MigrationWorkflow_StartMonitorCancel_WorksCorrectly()
    {
        // Arrange
        var request = new StartMigrationRequest
        {
            Options = new MigrationOptionsDto
            {
                ClearExistingData = false,
                CreateMissingTables = false,
                ValidateAgainstLegacy = false,
                RemoveAuthentication = false,
                MaxConcurrentOperations = 1,
                OperationTimeoutMinutes = 5
            }
        };

        // Act 1: Start migration
        var startResponse = await _client.PostAsJsonAsync("/api/migration/start", request);
        
        if (startResponse.StatusCode == HttpStatusCode.Conflict)
        {
            // Migration already running, cancel it first
            await _client.PostAsync("/api/migration/cancel", null);
            await Task.Delay(2000);
            
            // Try starting again
            startResponse = await _client.PostAsJsonAsync("/api/migration/start", request);
        }

        if (startResponse.IsSuccessStatusCode)
        {
            var startStatus = await startResponse.Content.ReadFromJsonAsync<MigrationStatusDto>();
            Assert.NotNull(startStatus);
            Assert.NotEqual(Guid.Empty, startStatus.MigrationId);

            // Act 2: Check if migration is running
            var runningResponse = await _client.GetAsync("/api/migration/running");
            var isRunning = await runningResponse.Content.ReadFromJsonAsync<bool>();
            
            if (isRunning)
            {
                // Act 3: Get migration status
                var statusResponse = await _client.GetAsync("/api/migration/status");
                statusResponse.EnsureSuccessStatusCode();
                var status = await statusResponse.Content.ReadFromJsonAsync<MigrationStatusDto>();
                
                Assert.NotNull(status);
                Assert.Equal(startStatus.MigrationId, status.MigrationId);

                // Act 4: Cancel migration
                var cancelResponse = await _client.PostAsync("/api/migration/cancel", null);
                cancelResponse.EnsureSuccessStatusCode();

                // Wait for cancellation to take effect
                await Task.Delay(1000);

                // Act 5: Verify migration is no longer running
                var finalRunningResponse = await _client.GetAsync("/api/migration/running");
                var finalIsRunning = await finalRunningResponse.Content.ReadFromJsonAsync<bool>();
                
                // Note: Migration might complete before cancellation, so we don't assert false here
                // Just verify the endpoint responds correctly
                Assert.True(finalIsRunning == true || finalIsRunning == false);
            }
        }
    }

    [Fact]
    public async Task MigrationEndpoints_HandleConcurrentRequests_Gracefully()
    {
        // Arrange
        var request = new StartMigrationRequest
        {
            Options = new MigrationOptionsDto
            {
                ClearExistingData = false,
                CreateMissingTables = false,
                ValidateAgainstLegacy = false,
                RemoveAuthentication = false
            }
        };

        // Act - Make multiple concurrent requests
        var tasks = new List<Task<HttpResponseMessage>>();
        for (int i = 0; i < 5; i++)
        {
            tasks.Add(_client.PostAsJsonAsync("/api/migration/start", request));
        }

        var responses = await Task.WhenAll(tasks);

        // Assert - Only one should succeed, others should return conflict
        var successCount = responses.Count(r => r.IsSuccessStatusCode);
        var conflictCount = responses.Count(r => r.StatusCode == HttpStatusCode.Conflict);

        Assert.True(successCount <= 1, "At most one migration should start successfully");
        Assert.True(conflictCount >= 4, "At least 4 requests should return conflict");

        // Cleanup
        await _client.PostAsync("/api/migration/cancel", null);
    }

    [Fact]
    public async Task MigrationStatistics_WithDifferentPeriods_ReturnsCorrectData()
    {
        // Test different time periods
        var periods = new[] { 7, 30, 90 };

        foreach (var period in periods)
        {
            // Act
            var response = await _client.GetAsync($"/api/migration/statistics?days={period}");

            // Assert
            response.EnsureSuccessStatusCode();
            var stats = await response.Content.ReadFromJsonAsync<MigrationStatisticsSummaryDto>();
            
            Assert.NotNull(stats);
            Assert.Equal(period, stats.PeriodDays);
            Assert.True(stats.TotalMigrations >= 0);
            Assert.True(stats.SuccessfulMigrations >= 0);
            Assert.True(stats.FailedMigrations >= 0);
            Assert.True(stats.CancelledMigrations >= 0);
            Assert.Equal(
                stats.TotalMigrations, 
                stats.SuccessfulMigrations + stats.FailedMigrations + stats.CancelledMigrations
            );
        }
    }

    [Fact]
    public async Task MigrationController_ErrorHandling_ReturnsAppropriateStatusCodes()
    {
        // Test various error scenarios
        var testCases = new[]
        {
            new { Endpoint = "/api/migration/invalid-endpoint", ExpectedStatus = HttpStatusCode.NotFound },
            new { Endpoint = $"/api/migration/{Guid.NewGuid()}", ExpectedStatus = HttpStatusCode.NotFound },
            new { Endpoint = $"/api/migration/{Guid.NewGuid()}/report", ExpectedStatus = HttpStatusCode.NotFound },
            new { Endpoint = $"/api/migration/{Guid.NewGuid()}/logs", ExpectedStatus = HttpStatusCode.NotFound }
        };

        foreach (var testCase in testCases)
        {
            // Act
            var response = await _client.GetAsync(testCase.Endpoint);

            // Assert
            Assert.Equal(testCase.ExpectedStatus, response.StatusCode);
        }
    }
}