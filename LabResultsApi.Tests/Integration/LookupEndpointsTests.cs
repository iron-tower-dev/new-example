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
public class LookupEndpointsTests
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

        if (!context.NasLookup.Any())
        {
            var nasLookups = new List<NasLookup>
            {
                new NasLookup { Id = 1, Channel = 1, ValLo = 100, ValHi = 199, NAS = 8 },
                new NasLookup { Id = 2, Channel = 1, ValLo = 200, ValHi = 299, NAS = 9 }
            };

            context.NasLookup.AddRange(nasLookups);
        }

        if (!context.NlgiLookup.Any())
        {
            var nlgiLookups = new List<NlgiLookup>
            {
                new NlgiLookup { ID = 1, LowerValue = 355, UpperValue = 385, NLGIValue = "00" },
                new NlgiLookup { ID = 2, LowerValue = 310, UpperValue = 340, NLGIValue = "0" },
                new NlgiLookup { ID = 3, LowerValue = 265, UpperValue = 295, NLGIValue = "1" }
            };

            context.NlgiLookup.AddRange(nlgiLookups);
        }

        context.SaveChanges();
    }

    [Test]
    public async Task GetNASLookup_ValidParticleCounts_ReturnsOkWithNASValue()
    {
        // Arrange
        var particleCounts = new Dictionary<string, int>
        {
            { "5-10", 150 },
            { "10-15", 75 },
            { "15-25", 35 },
            { "25-50", 15 },
            { "50-100", 7 },
            { ">100", 3 }
        };

        var json = JsonSerializer.Serialize(particleCounts);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/lookups/nas/calculate", content);

        // Assert
        response.Should().BeSuccessful();
        var result = await response.Content.ReadFromJsonAsync<NasLookupResult>();
        result.Should().NotBeNull();
        result.HighestNAS.Should().BeGreaterThan(0);
    }

    [Test]
    public async Task GetNLGILookup_ValidPenetrationValue_ReturnsOkWithGrade()
    {
        // Arrange
        var penetrationValue = 275;

        // Act
        var response = await _client.GetAsync($"/api/lookups/nlgi/penetration/{penetrationValue}");

        // Assert
        response.Should().BeSuccessful();
        var result = await response.Content.ReadFromJsonAsync<object>();
        result.Should().NotBeNull();
    }

    [Test]
    public async Task GetNLGILookup_OutOfRangePenetrationValue_ReturnsNotFound()
    {
        // Arrange
        var penetrationValue = 500.0;

        // Act
        var response = await _client.GetAsync($"/api/lookups/nlgi/{penetrationValue}");

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
    }

    [Test]
    public async Task GetParticleTypes_ReturnsOkWithParticleTypes()
    {
        // Act
        var response = await _client.GetAsync("/api/lookups/particle-types");

        // Assert
        response.Should().BeSuccessful();
        var result = await response.Content.ReadFromJsonAsync<List<object>>();
        result.Should().NotBeNull();
    }
}