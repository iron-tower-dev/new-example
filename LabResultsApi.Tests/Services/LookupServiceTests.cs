using NUnit.Framework;
using FluentAssertions;
using LabResultsApi.Services;

namespace LabResultsApi.Tests.Services;

[TestFixture]
public class LookupServiceTests : ServiceTestBase<ILookupService>
{
    [Test]
    public async Task GetNASLookupTableAsync_ReturnsNASLookupData()
    {
        // Act
        var result = await Service.GetNASLookupTableAsync();

        // Assert
        result.Should().NotBeNull();
    }

    [Test]
    public async Task GetNLGIForPenetrationAsync_ValidPenetrationValue_ReturnsGrade()
    {
        // Arrange
        var penetrationValue = 275;

        // Act
        var result = await Service.GetNLGIForPenetrationAsync(penetrationValue);

        // Assert
        result.Should().NotBeNull();
    }

    [Test]
    public async Task GetNLGILookupTableAsync_ReturnsNLGILookupData()
    {
        // Act
        var result = await Service.GetNLGILookupTableAsync();

        // Assert
        result.Should().NotBeNull();
    }

    [Test]
    public async Task GetParticleTypeDefinitionsAsync_ReturnsParticleTypes()
    {
        // Act
        var result = await Service.GetParticleTypeDefinitionsAsync();

        // Assert
        result.Should().NotBeNull();
    }

    [Test]
    public async Task GetCachedEquipmentByTypeAsync_ValidType_ReturnsEquipment()
    {
        // Arrange
        var equipType = "Thermometer";

        // Act
        var result = await Service.GetCachedEquipmentByTypeAsync(equipType);

        // Assert
        result.Should().NotBeNull();
    }
}