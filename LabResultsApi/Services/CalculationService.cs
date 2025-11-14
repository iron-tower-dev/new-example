using LabResultsApi.Models;
using LabResultsApi.DTOs;

namespace LabResultsApi.Services;

public class CalculationService : ICalculationService
{
    /// <summary>
    /// Calculate TAN (Total Acid Number) value
    /// Formula: (Final Buret * 5.61) / Sample Weight
    /// </summary>
    public double CalculateTAN(double sampleWeight, double finalBuret)
    {
        if (sampleWeight <= 0)
            throw new ArgumentException("Sample weight must be greater than zero", nameof(sampleWeight));
        
        if (finalBuret < 0)
            throw new ArgumentException("Final buret reading cannot be negative", nameof(finalBuret));

        var result = (finalBuret * 5.61) / sampleWeight;
        
        // Ensure minimum TAN value of 0.01
        return Math.Max(result, 0.01);
    }

    /// <summary>
    /// Calculate viscosity value
    /// Formula: Stopwatch Time * Tube Calibration Value
    /// </summary>
    public double CalculateViscosity(double stopwatchTime, double tubeCalibrationValue)
    {
        if (stopwatchTime <= 0)
            throw new ArgumentException("Stopwatch time must be greater than zero", nameof(stopwatchTime));
        
        if (tubeCalibrationValue <= 0)
            throw new ArgumentException("Tube calibration value must be greater than zero", nameof(tubeCalibrationValue));

        return stopwatchTime * tubeCalibrationValue;
    }

    /// <summary>
    /// Calculate corrected flash point
    /// Formula: Flash Point + ((Barometric Pressure - 760) * 0.03)
    /// </summary>
    public double CalculateFlashPoint(double flashPointTemp, double barometricPressure)
    {
        const double standardPressure = 760.0;
        const double correctionFactor = 0.03;
        
        return flashPointTemp + ((barometricPressure - standardPressure) * correctionFactor);
    }

    /// <summary>
    /// Calculate grease penetration value
    /// Formula: ((Average of 3 penetrations) * 3.75) + 24
    /// </summary>
    public double CalculateGreasePenetration(double pen1, double pen2, double pen3)
    {
        var average = (pen1 + pen2 + pen3) / 3.0;
        return (average * 3.75) + 24;
    }

    /// <summary>
    /// Calculate grease dropping point
    /// Formula: Dropping Point + ((Block Temp - Dropping Point) / 3)
    /// </summary>
    public double CalculateGreaseDroppingPoint(double droppingPoint, double blockTemp)
    {
        return droppingPoint + ((blockTemp - droppingPoint) / 3.0);
    }

    /// <summary>
    /// Round value to specified decimal places
    /// </summary>
    public double RoundToDecimalPlaces(double value, int decimalPlaces)
    {
        if (decimalPlaces < 0)
            throw new ArgumentException("Decimal places cannot be negative", nameof(decimalPlaces));

        return Math.Round(value, decimalPlaces, MidpointRounding.AwayFromZero);
    }

    // Interface implementations
    public async Task<TestCalculationResult> CalculateAsync(int testId, Dictionary<string, object?> inputValues)
    {
        try
        {
            return testId switch
            {
                1 => await CalculateTanFromValuesAsync(inputValues),
                2 => await CalculateViscosityFromValuesAsync(inputValues),
                3 => await CalculateFlashPointFromValuesAsync(inputValues),
                4 => await CalculateGreasePenetrationFromValuesAsync(inputValues),
                5 => await CalculateGreaseDroppingPointFromValuesAsync(inputValues),
                _ => new TestCalculationResult
                {
                    IsValid = false,
                    ErrorMessage = $"No calculation method defined for test ID {testId}"
                }
            };
        }
        catch (Exception ex)
        {
            return new TestCalculationResult
            {
                IsValid = false,
                ErrorMessage = ex.Message
            };
        }
    }

    public async Task<TestCalculationResult> CalculateTanAsync(double sampleWeight, double finalBuret)
    {
        try
        {
            var result = CalculateTAN(sampleWeight, finalBuret);
            return new TestCalculationResult
            {
                Result = Math.Round(result, 2),
                IsValid = true
            };
        }
        catch (Exception ex)
        {
            return new TestCalculationResult
            {
                IsValid = false,
                ErrorMessage = ex.Message
            };
        }
    }

    public async Task<TestCalculationResult> CalculateViscosityAsync(double stopwatchTime, double tubeCalibration)
    {
        try
        {
            var result = CalculateViscosity(stopwatchTime, tubeCalibration);
            return new TestCalculationResult
            {
                Result = Math.Round(result, 4),
                IsValid = true
            };
        }
        catch (Exception ex)
        {
            return new TestCalculationResult
            {
                IsValid = false,
                ErrorMessage = ex.Message
            };
        }
    }

    public async Task<TestCalculationResult> CalculateFlashPointAsync(double flashPointTemp, double barometricPressure)
    {
        try
        {
            var result = CalculateFlashPoint(flashPointTemp, barometricPressure);
            return new TestCalculationResult
            {
                Result = Math.Round(result, 0),
                IsValid = true
            };
        }
        catch (Exception ex)
        {
            return new TestCalculationResult
            {
                IsValid = false,
                ErrorMessage = ex.Message
            };
        }
    }

    public async Task<TestCalculationResult> CalculateGreasePenetrationAsync(double pen1, double pen2, double pen3)
    {
        try
        {
            var result = CalculateGreasePenetration(pen1, pen2, pen3);
            return new TestCalculationResult
            {
                Result = Math.Round(result, 0),
                IsValid = true
            };
        }
        catch (Exception ex)
        {
            return new TestCalculationResult
            {
                IsValid = false,
                ErrorMessage = ex.Message
            };
        }
    }

    public async Task<TestCalculationResult> CalculateGreaseDroppingPointAsync(double droppingPoint, double blockTemp)
    {
        try
        {
            var result = CalculateGreaseDroppingPoint(droppingPoint, blockTemp);
            return new TestCalculationResult
            {
                Result = Math.Round(result, 1),
                IsValid = true
            };
        }
        catch (Exception ex)
        {
            return new TestCalculationResult
            {
                IsValid = false,
                ErrorMessage = ex.Message
            };
        }
    }

    // Helper methods for parsing input values
    private async Task<TestCalculationResult> CalculateTanFromValuesAsync(Dictionary<string, object?> values)
    {
        if (!values.TryGetValue("sampleWeight", out var swObj) || !double.TryParse(swObj?.ToString(), out var sampleWeight))
            throw new ArgumentException("Invalid or missing sample weight");

        if (!values.TryGetValue("finalBuret", out var fbObj) || !double.TryParse(fbObj?.ToString(), out var finalBuret))
            throw new ArgumentException("Invalid or missing final buret reading");

        return await CalculateTanAsync(sampleWeight, finalBuret);
    }

    private async Task<TestCalculationResult> CalculateViscosityFromValuesAsync(Dictionary<string, object?> values)
    {
        if (!values.TryGetValue("stopwatchTime", out var stObj) || !double.TryParse(stObj?.ToString(), out var stopwatchTime))
            throw new ArgumentException("Invalid or missing stopwatch time");

        if (!values.TryGetValue("tubeCalibration", out var tcObj) || !double.TryParse(tcObj?.ToString(), out var tubeCalibration))
            throw new ArgumentException("Invalid or missing tube calibration value");

        return await CalculateViscosityAsync(stopwatchTime, tubeCalibration);
    }

    private async Task<TestCalculationResult> CalculateFlashPointFromValuesAsync(Dictionary<string, object?> values)
    {
        if (!values.TryGetValue("flashPointTemp", out var fpObj) || !double.TryParse(fpObj?.ToString(), out var flashPointTemp))
            throw new ArgumentException("Invalid or missing flash point temperature");

        if (!values.TryGetValue("barometricPressure", out var bpObj) || !double.TryParse(bpObj?.ToString(), out var barometricPressure))
            throw new ArgumentException("Invalid or missing barometric pressure");

        return await CalculateFlashPointAsync(flashPointTemp, barometricPressure);
    }

    private async Task<TestCalculationResult> CalculateGreasePenetrationFromValuesAsync(Dictionary<string, object?> values)
    {
        if (!values.TryGetValue("pen1", out var p1Obj) || !double.TryParse(p1Obj?.ToString(), out var pen1))
            throw new ArgumentException("Invalid or missing first penetration value");

        if (!values.TryGetValue("pen2", out var p2Obj) || !double.TryParse(p2Obj?.ToString(), out var pen2))
            throw new ArgumentException("Invalid or missing second penetration value");

        if (!values.TryGetValue("pen3", out var p3Obj) || !double.TryParse(p3Obj?.ToString(), out var pen3))
            throw new ArgumentException("Invalid or missing third penetration value");

        return await CalculateGreasePenetrationAsync(pen1, pen2, pen3);
    }

    private async Task<TestCalculationResult> CalculateGreaseDroppingPointFromValuesAsync(Dictionary<string, object?> values)
    {
        if (!values.TryGetValue("droppingPoint", out var dpObj) || !double.TryParse(dpObj?.ToString(), out var droppingPoint))
            throw new ArgumentException("Invalid or missing dropping point");

        if (!values.TryGetValue("blockTemp", out var btObj) || !double.TryParse(btObj?.ToString(), out var blockTemp))
            throw new ArgumentException("Invalid or missing block temperature");

        return await CalculateGreaseDroppingPointAsync(droppingPoint, blockTemp);
    }
}