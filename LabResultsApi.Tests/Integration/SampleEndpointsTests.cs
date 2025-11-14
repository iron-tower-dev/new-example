using NUnit.Framework;
using FluentAssertions;
using System.Net.Http.Json;
using LabResultsApi.DTOs;

namespace LabResultsApi.Tests.Integration;

[TestFixture]
public class SampleEndpointsTests : IntegrationTestBase
{
    [Test]
    public async Task GetSamplesByTest_ValidTestId_ReturnsOkWithSamples()
    {
        // Arrange
        var testId = 1;

        // Act
        var response = await Client.GetAsync($"/api/samples/by-test/{testId}");

        // Assert
        response.Should().BeSuccessful();
        var samples = await response.Content.ReadFromJsonAsync<List<SampleDto>>();
        samples.Should().NotBeNull();
    }

    [Test]
    public async Task GetSample_ValidSampleId_ReturnsOkWithSample()
    {
        // Arrange
        var sampleId = 1;

        // Act
        var response = await Client.GetAsync($"/api/samples/{sampleId}");

        // Assert
        response.Should().BeSuccessful();
        var sample = await response.Content.ReadFromJsonAsync<SampleDto>();
        sample.Should().NotBeNull();
        sample.Id.Should().Be(sampleId);
    }

    [Test]
    public async Task GetSample_InvalidSampleId_ReturnsNotFound()
    {
        // Arrange
        var sampleId = 999;

        // Act
        var response = await Client.GetAsync($"/api/samples/{sampleId}");

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
    }

    [Test]
    public async Task GetSampleHistory_ValidParameters_ReturnsOkWithHistory()
    {
        // Arrange
        var sampleId = 1;
        var testId = 1;

        // Act
        var response = await Client.GetAsync($"/api/samples/{sampleId}/history/{testId}");

        // Assert
        response.Should().BeSuccessful();
        var history = await response.Content.ReadFromJsonAsync<List<object>>();
        history.Should().NotBeNull();
    }
}