using NUnit.Framework;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using System.Net.Http.Json;
using LabResultsApi.Data;
using LabResultsApi.Models;
using LabResultsApi.DTOs;
using System.Text;
using System.Text.Json;

namespace LabResultsApi.Tests.Integration;

[TestFixture]
public class TestEndpointsTests
{
    private WebApplicationFactory<Program> _factory;
    private HttpClient _client;

    [SetUp]
    public void Setup()
    {
        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // Remove the real database context
                    var descriptor = services.SingleOrDefault(
                        d => d.ServiceType == typeof(DbContextOptions<LabDbContext>));
                    if (descriptor != null)
                        services.Remove(descriptor);

                    // Add in-memory database for testing
                    services.AddDbContext<LabDbContext>(options =>
                    {
                        options.UseInMemoryDatabase("TestDb");
                    });

                    // Build the service provider and seed test data
                    var sp = services.BuildServiceProvider();
                    using var scope = sp.CreateScope();
                    var context = scope.ServiceProvider.GetRequiredService<LabDbContext>();
                    SeedTestData(context);
                });
            });

        _client = _factory.CreateClient();
    }

    [TearDown]
    public void TearDown()
    {
        _client.Dispose();
        _factory.Dispose();
    }

    private static void SeedTestData(LabDbContext context)
    {
        context.Database.EnsureCreated();

        if (!context.Tests.Any())
        {
            var tests = new List<Test>
            {
                new Test
                {
                    TestId = 1,
                    TestName = "TAN",
                    TestDescription = "Total Acid Number",
                    Active = true
                },
                new Test
                {
                    TestId = 2,
                    TestName = "Viscosity",
                    TestDescription = "Kinematic Viscosity",
                    Active = true
                }
            };

            context.Tests.AddRange(tests);
            context.SaveChanges();
        }
    }

    [Test]
    public async Task GetTests_ReturnsOkWithTests()
    {
        // Act
        var response = await _client.GetAsync("/api/tests");

        // Assert
        response.Should().BeSuccessful();
        var tests = await response.Content.ReadFromJsonAsync<List<TestDto>>();
        tests.Should().NotBeNull();
        tests.Should().HaveCountGreaterThan(0);
    }

    [Test]
    public async Task GetTestTemplate_ValidTestId_ReturnsOkWithTemplate()
    {
        // Arrange
        var testId = 1;

        // Act
        var response = await _client.GetAsync($"/api/tests/{testId}/template");

        // Assert
        response.Should().BeSuccessful();
        var template = await response.Content.ReadFromJsonAsync<TestTemplateDto>();
        template.Should().NotBeNull();
        template.TestId.Should().Be(testId);
    }

    [Test]
    public async Task SaveTestResults_ValidData_ReturnsOkWithResult()
    {
        // Arrange
        var testId = 1;
        var testResult = new SaveTestResultRequest
        {
            SampleId = 1,
            TestId = testId,
            Trials = new List<TrialResultDto>
            {
                new TrialResultDto
                {
                    TrialNumber = 1,
                    Values = new Dictionary<string, object?>
                    {
                        { "SampleWeight", 10.0 },
                        { "FinalBuret", 5.0 }
                    }
                }
            },
            EntryId = "TEST_USER"
        };

        var json = JsonSerializer.Serialize(testResult);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync($"/api/tests/{testId}/results", content);

        // Assert
        response.Should().BeSuccessful();
        var result = await response.Content.ReadFromJsonAsync<object>();
        result.Should().NotBeNull();
    }

    [Test]
    public async Task UpdateTestResults_ValidData_ReturnsOkWithResult()
    {
        // Arrange
        var testId = 1;
        var sampleId = 1;
        var testResult = new SaveTestResultRequest
        {
            SampleId = sampleId,
            TestId = testId,
            Trials = new List<TrialResultDto>
            {
                new TrialResultDto
                {
                    TrialNumber = 1,
                    Values = new Dictionary<string, object?>
                    {
                        { "SampleWeight", 12.0 },
                        { "FinalBuret", 6.0 }
                    }
                }
            },
            EntryId = "TEST_USER"
        };

        var json = JsonSerializer.Serialize(testResult);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PutAsync($"/api/tests/{testId}/results/{sampleId}", content);

        // Assert
        response.Should().BeSuccessful();
        var result = await response.Content.ReadFromJsonAsync<object>();
        result.Should().NotBeNull();
    }

    [Test]
    public async Task DeleteTestResults_ValidParameters_ReturnsOk()
    {
        // Arrange
        var testId = 1;
        var sampleId = 1;

        // Act
        var response = await _client.DeleteAsync($"/api/tests/{testId}/results/{sampleId}");

        // Assert
        response.Should().BeSuccessful();
    }
}