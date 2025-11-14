using NUnit.Framework;
using FluentAssertions;
using LabResultsApi.Services;

namespace LabResultsApi.Tests.Services;

[TestFixture]
public class SampleServiceTests : ServiceTestBase<ISampleService>
{
    [Test]
    public async Task GetSamplesByTestAsync_ValidTestId_ReturnsSamples()
    {
        // Arrange
        var testId = 1;

        // Act
        var result = await Service.GetSamplesByTestAsync(testId);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCountGreaterThan(0);
    }

    [Test]
    public async Task GetSampleAsync_ValidSampleId_ReturnsSample()
    {
        // Arrange
        var sampleId = 1;

        // Act
        var result = await Service.GetSampleAsync(sampleId);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(sampleId);
        result.TagNumber.Should().Be("TEST001");
    }

    [Test]
    public async Task GetSampleAsync_InvalidSampleId_ReturnsNull()
    {
        // Arrange
        var sampleId = 999;

        // Act
        var result = await Service.GetSampleAsync(sampleId);

        // Assert
        result.Should().BeNull();
    }

    [Test]
    public async Task GetSampleHistoryAsync_ValidParameters_ReturnsHistory()
    {
        // Arrange
        var sampleId = 1;
        var testId = 1;

        // Act
        var result = await Service.GetSampleHistoryAsync(sampleId, testId);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<List<object>>();
    }
}