using LabResultsApi.DTOs;

namespace LabResultsApi.Services;

public interface ICalculationService
{
    Task<TestCalculationResult> CalculateAsync(int testId, Dictionary<string, object?> inputValues);
    Task<TestCalculationResult> CalculateTanAsync(double sampleWeight, double finalBuret);
    Task<TestCalculationResult> CalculateViscosityAsync(double stopwatchTime, double tubeCalibration);
    Task<TestCalculationResult> CalculateFlashPointAsync(double flashPointTemp, double barometricPressure);
    Task<TestCalculationResult> CalculateGreasePenetrationAsync(double pen1, double pen2, double pen3);
    Task<TestCalculationResult> CalculateGreaseDroppingPointAsync(double droppingPoint, double blockTemp);
}