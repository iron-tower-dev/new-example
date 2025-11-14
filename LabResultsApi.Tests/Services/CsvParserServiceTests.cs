using NUnit.Framework;
using FluentAssertions;
using LabResultsApi.Services;
using LabResultsApi.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Text;

namespace LabResultsApi.Tests.Services;

[TestFixture]
public class CsvParserServiceTests : ServiceTestBase<ICsvParserService>
{
    private string _testDirectory = null!;

    protected override void RegisterServices(IServiceCollection services)
    {
        base.RegisterServices();
        services.AddScoped<ICsvParserService, CsvParserService>();
    }

    [OneTimeSetUp]
    public override async Task OneTimeSetUp()
    {
        await base.OneTimeSetUp();
        
        _testDirectory = Path.Combine(Path.GetTempPath(), "csv-parser-tests");
        Directory.CreateDirectory(_testDirectory);
        
        await CreateTestFiles();
    }

    [OneTimeTearDown]
    public override async Task OneTimeTearDown()
    {
        if (Directory.Exists(_testDirectory))
        {
            Directory.Delete(_testDirectory, true);
        }
        
        await base.OneTimeTearDown();
    }

    [Test]
    public async Task ParseCsvFileAsync_ValidFile_ShouldParseSuccessfully()
    {
        // Arrange
        var filePath = Path.Combine(_testDirectory, "valid.csv");
        var options = new CsvParseOptions { HasHeader = true };

        // Act
        var result = await Service.ParseCsvFileAsync(filePath, options);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Should().HaveCount(2);
        result.Headers.Should().Equal("ID", "Name", "Active");
        result.ValidRows.Should().Be(2);
        result.ErrorRows.Should().Be(0);
        result.Errors.Should().BeEmpty();
    }

    [Test]
    public async Task ParseCsvFileAsync_NonExistentFile_ShouldReturnError()
    {
        // Arrange
        var filePath = Path.Combine(_testDirectory, "nonexistent.csv");
        var options = new CsvParseOptions { HasHeader = true };

        // Act
        var result = await Service.ParseCsvFileAsync(filePath, options);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Errors.Should().HaveCount(1);
        result.Errors[0].ErrorType.Should().Be("FileNotFound");
    }

    [Test]
    public async Task ParseCsvFileAsync_WithTypeConversion_ShouldConvertTypes()
    {
        // Arrange
        var filePath = Path.Combine(_testDirectory, "typed.csv");
        var options = new CsvParseOptions
        {
            HasHeader = true,
            ColumnTypes = new Dictionary<string, Type>
            {
                { "ID", typeof(int) },
                { "Value", typeof(double) },
                { "Active", typeof(bool) }
            }
        };

        // Act
        var result = await Service.ParseCsvFileAsync(filePath, options);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data[0]["ID"].Should().BeOfType<int>().And.Be(1);
        result.Data[0]["Value"].Should().BeOfType<double>().And.Be(10.5);
        result.Data[0]["Active"].Should().BeOfType<bool>().And.Be(true);
    }

    [Test]
    public async Task ParseCsvStreamAsync_ValidStream_ShouldParseSuccessfully()
    {
        // Arrange
        var csvContent = "ID,Name\n1,Test1\n2,Test2";
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(csvContent));
        var options = new CsvParseOptions { HasHeader = true };

        // Act
        var result = await Service.ParseCsvStreamAsync(stream, "test.csv", options);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Should().HaveCount(2);
        result.Headers.Should().Equal("ID", "Name");
    }

    [Test]
    public void ValidateCsvData_ValidData_ShouldPassValidation()
    {
        // Arrange
        var data = new List<Dictionary<string, object>>
        {
            new() { { "ID", 1 }, { "Name", "Test1" } },
            new() { { "ID", 2 }, { "Name", "Test2" } }
        };

        var schema = new CsvSchema
        {
            TableName = "TestTable",
            Columns = new List<CsvColumnDefinition>
            {
                new() { Name = "ID", DataType = typeof(int), IsRequired = true },
                new() { Name = "Name", DataType = typeof(string), IsRequired = true, MaxLength = 50 }
            }
        };

        // Act
        var result = Service.ValidateCsvData(data, schema);

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
        result.ValidatedRows.Should().Be(2);
    }

    [Test]
    public void ValidateCsvData_InvalidData_ShouldFailValidation()
    {
        // Arrange
        var data = new List<Dictionary<string, object>>
        {
            new() { { "ID", 1 }, { "Name", "Test1" } },
            new() { { "ID", null }, { "Name", "" } }, // Invalid row
            new() { { "ID", 3 }, { "Name", new string('x', 100) } } // Too long
        };

        var schema = new CsvSchema
        {
            TableName = "TestTable",
            Columns = new List<CsvColumnDefinition>
            {
                new() { Name = "ID", DataType = typeof(int), IsRequired = true, AllowNull = false },
                new() { Name = "Name", DataType = typeof(string), IsRequired = true, MaxLength = 50 }
            }
        };

        // Act
        var result = Service.ValidateCsvData(data, schema);

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(2); // Null ID and too long name
        result.ValidatedRows.Should().Be(3);
    }

    [Test]
    public void ConvertToTypedObject_ValidData_ShouldConvertSuccessfully()
    {
        // Arrange
        var rowData = new Dictionary<string, object>
        {
            { "Id", 1 },
            { "Name", "Test" },
            { "IsActive", true }
        };

        // Act
        var result = Service.ConvertToTypedObject<TestModel>(rowData);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(1);
        result.Name.Should().Be("Test");
        result.IsActive.Should().BeTrue();
    }

    [Test]
    public void GetSchemaForTable_KnownTable_ShouldReturnSchema()
    {
        // Act
        var result = Service.GetSchemaForTable("test");

        // Assert
        result.Should().NotBeNull();
        result.TableName.Should().Be("Test");
        result.Columns.Should().NotBeEmpty();
        result.ValidationRules.Should().NotBeEmpty();
    }

    [Test]
    public void GetSchemaForTable_UnknownTable_ShouldReturnDefaultSchema()
    {
        // Act
        var result = Service.GetSchemaForTable("unknown");

        // Assert
        result.Should().NotBeNull();
        result.TableName.Should().Be("unknown");
        result.Columns.Should().BeEmpty();
        result.ValidationRules.Should().BeEmpty();
    }

    [Test]
    public async Task ProcessInBatchesAsync_LargeFile_ShouldReturnBatches()
    {
        // Arrange
        var filePath = Path.Combine(_testDirectory, "large.csv");
        var batchSize = 2;
        var options = new CsvParseOptions { HasHeader = true };

        // Act
        var batches = new List<CsvBatch>();
        await foreach (var batch in Service.ProcessInBatchesAsync(filePath, batchSize, options))
        {
            batches.Add(batch);
        }

        // Assert
        batches.Should().HaveCount(2); // 3 records / 2 batch size = 2 batches
        batches[0].Data.Should().HaveCount(2);
        batches[1].Data.Should().HaveCount(1);
        batches[1].IsLastBatch.Should().BeTrue();
    }

    private async Task CreateTestFiles()
    {
        // Valid CSV file
        var validCsv = "ID,Name,Active\n1,Test1,1\n2,Test2,0";
        await File.WriteAllTextAsync(Path.Combine(_testDirectory, "valid.csv"), validCsv);

        // Typed CSV file
        var typedCsv = "ID,Value,Active\n1,10.5,1\n2,20.0,0";
        await File.WriteAllTextAsync(Path.Combine(_testDirectory, "typed.csv"), typedCsv);

        // Large CSV file
        var largeCsv = new StringBuilder("ID,Name\n");
        for (int i = 1; i <= 3; i++)
        {
            largeCsv.AppendLine($"{i},Test{i}");
        }
        await File.WriteAllTextAsync(Path.Combine(_testDirectory, "large.csv"), largeCsv.ToString());
    }

    public class TestModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }
}