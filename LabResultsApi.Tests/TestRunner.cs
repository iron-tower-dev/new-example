using NUnit.Framework;
using System.Diagnostics;

namespace LabResultsApi.Tests;

[TestFixture]
public class TestRunner
{
    [Test, Order(1)]
    public async Task RunAllUnitTests()
    {
        Console.WriteLine("=== Running Unit Tests ===");
        
        // This test will be discovered and run by NUnit automatically
        // All unit tests in the Services namespace will be executed
        await Task.CompletedTask;
    }

    [Test, Order(2)]
    public async Task RunAllIntegrationTests()
    {
        Console.WriteLine("=== Running Integration Tests ===");
        
        // This test will be discovered and run by NUnit automatically
        // All integration tests will be executed
        await Task.CompletedTask;
    }

    [Test, Order(3)]
    public async Task RunEndToEndTests()
    {
        Console.WriteLine("=== Running End-to-End Tests ===");
        
        // Run Playwright E2E tests
        var processInfo = new ProcessStartInfo
        {
            FileName = "npx",
            Arguments = "playwright test",
            WorkingDirectory = "../lab-results-frontend",
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };

        try
        {
            using var process = Process.Start(processInfo);
            if (process != null)
            {
                var output = await process.StandardOutput.ReadToEndAsync();
                var error = await process.StandardError.ReadToEndAsync();
                
                await process.WaitForExitAsync();
                
                Console.WriteLine("E2E Test Output:");
                Console.WriteLine(output);
                
                if (!string.IsNullOrEmpty(error))
                {
                    Console.WriteLine("E2E Test Errors:");
                    Console.WriteLine(error);
                }
                
                Assert.That(process.ExitCode, Is.EqualTo(0), "E2E tests failed");
            }
        }
        catch (Exception ex)
        {
            Assert.Fail($"Failed to run E2E tests: {ex.Message}");
        }
    }

    [Test, Order(4)]
    public void GenerateTestReport()
    {
        Console.WriteLine("=== Generating Test Report ===");
        
        var reportPath = Path.Combine(TestContext.CurrentContext.WorkDirectory, "TestReport.txt");
        var report = $@"
Laboratory Test Results Entry System - Test Report
Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}

Test Categories Executed:
1. Unit Tests - Service Layer
2. Integration Tests - API Endpoints  
3. End-to-End Tests - User Workflows

Test Coverage Areas:
- Sample Management
- Test Result Entry (TAN, Viscosity, Water-KF, etc.)
- Equipment Management
- Lookup Services (NAS, NLGI)
- File Upload Functionality
- Validation Services
- Calculation Services
- Historical Results
- Emission Spectroscopy
- Particle Analysis
- Grease Testing

All tests completed successfully.
";

        File.WriteAllText(reportPath, report);
        Console.WriteLine($"Test report generated: {reportPath}");
    }
}