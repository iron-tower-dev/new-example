using NUnit.Framework;
using FluentAssertions;
using LabResultsApi.Services;
using LabResultsApi.Data;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace LabResultsApi.Tests;

[TestFixture]
public class BasicCalculationTests
{
    [TestFixture]
    public class CalculationServiceTests
    {
        private ICalculationService _calculationService;
        private Mock<ILogger<CalculationService>> _mockLogger;

        [SetUp]
        public void Setup()
        {
            _mockLogger = new Mock<ILogger<CalculationService>>();
            _calculationService = new CalculationService();
        }

        [Test]
        public async Task CalculateTanAsync_ValidInputs_ReturnsCorrectResult()
        {
            // Arrange
            var sampleWeight = 10.0;
            var finalBuret = 5.0;
            var expectedResult = (finalBuret * 5.61) / sampleWeight;

            // Act
            var result = await _calculationService.CalculateTanAsync(sampleWeight, finalBuret);

            // Assert
            result.Should().NotBeNull();
            result.IsValid.Should().BeTrue();
            result.Result.Should().BeApproximately(expectedResult, 0.01);
        }

        [Test]
        public async Task CalculateViscosityAsync_ValidInputs_ReturnsCorrectResult()
        {
            // Arrange
            var stopwatchTime = 120.5;
            var tubeCalibrationValue = 0.1234;
            var expectedResult = stopwatchTime * tubeCalibrationValue;

            // Act
            var result = await _calculationService.CalculateViscosityAsync(stopwatchTime, tubeCalibrationValue);

            // Assert
            result.Should().NotBeNull();
            result.IsValid.Should().BeTrue();
            result.Result.Should().BeApproximately(expectedResult, 0.0001);
        }

        [Test]
        public async Task CalculateFlashPointAsync_ValidInputs_ReturnsCorrectResult()
        {
            // Arrange
            var flashPointTemp = 200.0;
            var barometricPressure = 750.0;
            var expectedResult = flashPointTemp + (0.06 * (760 - barometricPressure));

            // Act
            var result = await _calculationService.CalculateFlashPointAsync(flashPointTemp, barometricPressure);

            // Assert
            result.Should().NotBeNull();
            result.IsValid.Should().BeTrue();
            result.Result.Should().BeApproximately(expectedResult, 1.0);
        }

        [Test]
        public async Task CalculateGreasePenetrationAsync_ValidInputs_ReturnsCorrectResult()
        {
            // Arrange
            var penetration1 = 250.0;
            var penetration2 = 260.0;
            var penetration3 = 255.0;
            var expectedAverage = (penetration1 + penetration2 + penetration3) / 3;
            var expectedResult = (expectedAverage * 3.75) + 24;

            // Act
            var result = await _calculationService.CalculateGreasePenetrationAsync(penetration1, penetration2, penetration3);

            // Assert
            result.Should().NotBeNull();
            result.IsValid.Should().BeTrue();
            result.Result.Should().BeApproximately(expectedResult, 0.5);
        }

        [Test]
        public async Task CalculateGreaseDroppingPointAsync_ValidInputs_ReturnsCorrectResult()
        {
            // Arrange
            var droppingPoint = 180.0;
            var blockTemp = 185.0;
            var expectedResult = droppingPoint + ((blockTemp - droppingPoint) / 3);

            // Act
            var result = await _calculationService.CalculateGreaseDroppingPointAsync(droppingPoint, blockTemp);

            // Assert
            result.Should().NotBeNull();
            result.IsValid.Should().BeTrue();
            result.Result.Should().BeApproximately(expectedResult, 0.1);
        }
    }

    [TestFixture]
    public class ValidationServiceTests
    {
        private IValidationService _validationService;
        private Mock<ILogger<ValidationService>> _mockLogger;
        private LabDbContext _mockContext;

        [SetUp]
        public void Setup()
        {
            _mockLogger = new Mock<ILogger<ValidationService>>();
            
            // Create in-memory database for testing
            var options = new DbContextOptionsBuilder<LabDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            
            _mockContext = new LabDbContext(options);
            _validationService = new ValidationService(_mockContext, _mockLogger.Object);
        }

        [Test]
        public void GetTestValidationRules_ValidTestId_ReturnsRules()
        {
            // Arrange
            var testId = 1; // TAN test

            // Act
            var result = _validationService.GetTestValidationRules(testId);

            // Assert
            result.Should().NotBeNull();
            result.TestId.Should().Be(testId);
            result.FieldRules.Should().NotBeEmpty();
        }

        [Test]
        public void ValidateTrialData_ValidTanData_ReturnsValid()
        {
            // Arrange
            var testId = 1; // TAN test
            var trialData = new { SampleWeight = 10.0, FinalBuret = 5.0 };
            var trialNumber = 1;

            // Act
            var result = _validationService.ValidateTrialData(testId, trialData, trialNumber);

            // Assert
            result.Should().NotBeNull();
            result.IsValid.Should().BeTrue();
        }

        [Test]
        public void ValidateTrialData_InvalidTanData_ReturnsInvalid()
        {
            // Arrange
            var sampleWeight = 0.0; // Invalid - must be greater than zero
            var finalBuret = -1.0;  // Invalid - cannot be negative

            // Act
            var result = _validationService.ValidateTanData(sampleWeight, finalBuret);

            // Assert
            result.Should().NotBeNull();
            result.IsValid.Should().BeFalse();
            result.HasErrors.Should().BeTrue();
        }

        [Test]
        public void ValidateCrossFieldRules_ValidFieldCombination_ReturnsValid()
        {
            // Arrange
            var testId = 1;
            var fieldValues = new Dictionary<string, object>
            {
                { "SampleWeight", 10.0 },
                { "FinalBuret", 5.0 }
            };

            // Act
            var result = _validationService.ValidateCrossFieldRules(testId, fieldValues);

            // Assert
            result.Should().NotBeNull();
            result.IsValid.Should().BeTrue();
        }

        [Test]
        public void GetFieldValidationRules_ValidField_ReturnsRules()
        {
            // Arrange
            var testId = 1;
            var fieldName = "sampleWeight"; // Use the correct field name from the validation rules

            // Act
            var result = _validationService.GetFieldValidationRules(testId, fieldName);

            // Assert
            result.Should().NotBeNull();
            result.Should().NotBeEmpty();
        }
    }


}