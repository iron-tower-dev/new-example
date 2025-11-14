using NUnit.Framework;
using FluentAssertions;
using LabResultsApi.Services;
using LabResultsApi.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Text;

namespace LabResultsApi.Tests.Integration;

[TestFixture]
public class DatabaseSeedingTests : TestBase
{
    private ICsvParserService _csvParserService = null!;
    private ITableCreationService _tableCreationService = null!;
    private IDatabaseSeedingService _databaseSeedingService = null!;
    private string _testCsvDirectory = null!;
    private string _testSqlDirectory = null!;

    protected override void RegisterServices(IServiceCollection services)
    {
        base.RegisterServices();
        
        // Register the seeding services
        services.AddScoped<ICsvParserService, CsvParserService>();
        services.AddScoped<ITableCreationService, TableCreationService>();
        services.AddScoped<IDatabaseSeedingService, DatabaseSeedingService>();
    }

    [OneTimeSetUp]
    public override async Task OneTimeSetUp()
    {
        await base.OneTimeSetUp();
        
        _csvParserService = GetService<ICsvParserService>();
        _tableCreationService = GetService<ITableCreationService>();
        _databaseSeedingService = GetService<IDatabaseSeedingService>();
        
        // Create test directories
        _testCsvDirectory = Path.Combine(Path.GetTempPath(), "test-csv-data");
        _testSqlDirectory = Path.Combine(Path.GetTempPath(), "test-sql-scripts");
        
        Directory.CreateDirectory(_testCsvDirectory);
        Directory.CreateDirectory(_testSqlDirectory);
        
        // Create test files
        await CreateTestCsvFiles();
        await CreateTestSqlFiles();
    }

    [OneTimeTearDown]
    public override async Task OneTimeTearDown()
    {
        // Clean up test directories
        if (Directory.Exists(_testCsvDirectory))
        {
            Directory.Delete(_testCsvDirectory, true);
        }
        
        if (Directory.Exists(_testSqlDirectory))
        {
            Directory.Delete(_testSqlDirectory, true);
        }
        
        await base.OneTimeTearDown();
    }

    [Test]
    public async Task CsvParserService_ParseValidCsvFile_ShouldReturnSuccessfulResult()
    {
        // Arrange
        var csvFilePath = Path.Combine(_testCsvDirectory, "test-data.csv");
        var options = new CsvParseOptions
        {
            HasHeader = true,
            ContinueOnError = true,
            TrimWhitespace = true
        };

        // Act
        var result = await _csvParserService.ParseCsvFileAsync(csvFilePath, options);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Should().HaveCount(3);
        result.Headers.Should().Contain(new[] { "ID", "Name", "Value" });
        result.Errors.Should().BeEmpty();
        result.ValidRows.Should().Be(3);
        result.ErrorRows.Should().Be(0);
    }

    [Test]
    public async Task CsvParserService_ParseCsvWithErrors_ShouldContinueOnError()
    {
        // Arrange
        var csvFilePath = Path.Combine(_testCsvDirectory, "test-data-with-errors.csv");
        var options = new CsvParseOptions
        {
            HasHeader = true,
            ContinueOnError = true,
            ColumnTypes = new Dictionary<string, Type>
            {
                { "ID", typeof(int) },
                { "Value", typeof(double) }
            }
        };

        // Act
        var result = await _csvParserService.ParseCsvFileAsync(csvFilePath, options);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue(); // Should succeed with ContinueOnError = true
        result.Data.Should().HaveCount(4); // All rows should be parsed
        result.ValidRows.Should().Be(4);
        result.Errors.Should().NotBeEmpty(); // Should have conversion errors
    }

    [Test]
    public async Task CsvParserService_ProcessInBatches_ShouldReturnCorrectBatches()
    {
        // Arrange
        var csvFilePath = Path.Combine(_testCsvDirectory, "large-test-data.csv");
        var batchSize = 2;
        var options = new CsvParseOptions { HasHeader = true };

        // Act
        var batches = new List<CsvBatch>();
        await foreach (var batch in _csvParserService.ProcessInBatchesAsync(csvFilePath, batchSize, options))
        {
            batches.Add(batch);
        }

        // Assert
        batches.Should().HaveCount(3); // 5 records / 2 batch size = 3 batches
        batches[0].Data.Should().HaveCount(2);
        batches[1].Data.Should().HaveCount(2);
        batches[2].Data.Should().HaveCount(1);
        batches[2].IsLastBatch.Should().BeTrue();
    }

    [Test]
    public async Task CsvParserService_ValidateData_ShouldDetectValidationErrors()
    {
        // Arrange
        var data = new List<Dictionary<string, object>>
        {
            new() { { "ID", 1 }, { "Name", "Test1" }, { "Value", 10.5 } },
            new() { { "ID", -1 }, { "Name", "" }, { "Value", -5.0 } }, // Invalid data
            new() { { "ID", 3 }, { "Name", "Test3" }, { "Value", 15.0 } }
        };

        var schema = new CsvSchema
        {
            TableName = "TestTable",
            Columns = new List<CsvColumnDefinition>
            {
                new() { Name = "ID", DataType = typeof(int), IsRequired = true },
                new() { Name = "Name", DataType = typeof(string), IsRequired = true, MaxLength = 50 },
                new() { Name = "Value", DataType = typeof(double), IsRequired = true }
            },
            ValidationRules = new List<CsvValidationRule>
            {
                new()
                {
                    ColumnName = "ID",
                    RuleType = "positive",
                    ErrorMessage = "ID must be positive",
                    IsWarning = false
                },
                new()
                {
                    ColumnName = "Value",
                    RuleType = "positive",
                    ErrorMessage = "Value must be positive",
                    IsWarning = false
                }
            }
        };

        // Act
        var result = _csvParserService.ValidateCsvData(data, schema);

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(3); // ID negative, Name empty, Value negative
        result.ValidatedRows.Should().Be(3);
    }

    [Test]
    public async Task TableCreationService_TableExists_ShouldReturnCorrectStatus()
    {
        // Arrange
        var existingTableName = "UsedLubeSamples"; // This table should exist from test setup
        var nonExistentTableName = "NonExistentTable";

        // Act
        var existsResult = await _tableCreationService.TableExistsAsync(existingTableName);
        var notExistsResult = await _tableCreationService.TableExistsAsync(nonExistentTableName);

        // Assert
        existsResult.Should().BeTrue();
        notExistsResult.Should().BeFalse();
    }

    [Test]
    public async Task TableCreationService_ValidateSqlScript_ShouldReturnValidationResult()
    {
        // Arrange
        var scriptPath = Path.Combine(_testSqlDirectory, "TestTable.sql");

        // Act
        var result = await _tableCreationService.ValidateSqlScriptAsync(scriptPath);

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeTrue();
        result.DetectedTables.Should().Contain("TestTable");
        result.Errors.Should().BeEmpty();
    }

    [Test]
    public async Task TableCreationService_ResolveDependencies_ShouldReturnDependencyMap()
    {
        // Act
        var dependencies = await _tableCreationService.ResolveDependenciesAsync();

        // Assert
        dependencies.Should().NotBeNull();
        dependencies.Should().NotBeEmpty();
        
        // TestTableWithFK should depend on TestTable
        if (dependencies.ContainsKey("TestTableWithFK"))
        {
            dependencies["TestTableWithFK"].Should().Contain("TestTable");
        }
    }

    [Test]
    public async Task DatabaseSeedingService_GetAvailableCsvFiles_ShouldReturnTestFiles()
    {
        // Act
        var csvFiles = await _databaseSeedingService.GetAvailableCsvFilesAsync();

        // Assert
        csvFiles.Should().NotBeNull();
        csvFiles.Should().NotBeEmpty();
    }

    [Test]
    public async Task DatabaseSeedingService_MapCsvFileToTableName_ShouldReturnCorrectMapping()
    {
        // Arrange
        var testCases = new Dictionary<string, string>
        {
            { "test.csv", "Test" },
            { "testlist.csv", "TestList" },
            { "custom-table.csv", "custom-table" }
        };

        // Act & Assert
        foreach (var testCase in testCases)
        {
            var result = _databaseSeedingService.MapCsvFileToTableName(testCase.Key);
            result.Should().Be(testCase.Value);
        }
    }

    [Test]
    public async Task DatabaseSeedingService_ClearTableData_ShouldClearSpecifiedTables()
    {
        // Arrange
        var tableNames = new List<string> { "UsedLubeSamples" };

        // Act
        var result = await _databaseSeedingService.ClearTableDataAsync(tableNames);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.ClearedTables.Should().Contain("UsedLubeSamples");
        result.RecordsDeleted.Should().ContainKey("UsedLubeSamples");
        result.Errors.Should().BeEmpty();
    }

    [Test]
    public async Task DatabaseSeedingService_ValidateDataIntegrity_ShouldReturnValidationResult()
    {
        // Act
        var result = await _databaseSeedingService.ValidateDataIntegrityAsync();

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeTrue();
    }

    [Test]
    public async Task DatabaseSeedingService_GetSeedingStatistics_ShouldReturnStatistics()
    {
        // Act
        var statistics = await _databaseSeedingService.GetSeedingStatisticsAsync();

        // Assert
        statistics.Should().NotBeNull();
        statistics.TotalTables.Should().BeGreaterThan(0);
        statistics.TableStatistics.Should().NotBeEmpty();
    }

    private async Task CreateTestCsvFiles()
    {
        // Create basic test CSV file
        var testCsvContent = @"ID,Name,Value
1,Test1,10.5
2,Test2,20.0
3,Test3,30.5";
        await File.WriteAllTextAsync(Path.Combine(_testCsvDirectory, "test-data.csv"), testCsvContent);

        // Create CSV file with errors
        var errorCsvContent = @"ID,Name,Value
1,Test1,10.5
invalid,Test2,20.0
3,,invalid_number
4,Test4,40.0";
        await File.WriteAllTextAsync(Path.Combine(_testCsvDirectory, "test-data-with-errors.csv"), errorCsvContent);

        // Create larger CSV file for batch testing
        var largeCsvContent = new StringBuilder("ID,Name,Value\n");
        for (int i = 1; i <= 5; i++)
        {
            largeCsvContent.AppendLine($"{i},Test{i},{i * 10.0}");
        }
        await File.WriteAllTextAsync(Path.Combine(_testCsvDirectory, "large-test-data.csv"), largeCsvContent.ToString());

        // Create test.csv for mapping test
        var testTableCsv = @"ID,TestName,Active
1,Sample Test,1
2,Another Test,0";
        await File.WriteAllTextAsync(Path.Combine(_testCsvDirectory, "test.csv"), testTableCsv);
    }

    private async Task CreateTestSqlFiles()
    {
        // Create basic table creation script
        var testTableSql = @"
CREATE TABLE [TestTable] (
    [ID] int NOT NULL,
    [Name] nvarchar(50) NOT NULL,
    [Value] decimal(10,2) NULL,
    [CreatedDate] datetime2 DEFAULT GETDATE()
);";
        await File.WriteAllTextAsync(Path.Combine(_testSqlDirectory, "TestTable.sql"), testTableSql);

        // Create table with foreign key dependency
        var testTableWithFKSql = @"
CREATE TABLE [TestTableWithFK] (
    [ID] int NOT NULL,
    [TestTableID] int NOT NULL,
    [Description] nvarchar(100) NULL,
    FOREIGN KEY (TestTableID) REFERENCES TestTable(ID)
);";
        await File.WriteAllTextAsync(Path.Combine(_testSqlDirectory, "TestTableWithFK.sql"), testTableWithFKSql);

        // Create script with potential issues for validation testing
        var problematicSql = @"
-- This script has some warnings
USE [SomeDatabase]
GO

CREATE TABLE [ProblematicTable] (
    [ID] int NOT NULL,
    [Data] nvarchar(max) NULL
);

-- Potentially dangerous operation
-- DELETE FROM SomeTable;
";
        await File.WriteAllTextAsync(Path.Combine(_testSqlDirectory, "ProblematicTable.sql"), problematicSql);
    }
}