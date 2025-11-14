using NUnit.Framework;
using FluentAssertions;
using LabResultsApi.Services;
using LabResultsApi.Models;
using Microsoft.Extensions.DependencyInjection;

namespace LabResultsApi.Tests.Services;

[TestFixture]
public class TableCreationServiceTests : ServiceTestBase<ITableCreationService>
{
    private string _testSqlDirectory = null!;

    protected override void RegisterServices(IServiceCollection services)
    {
        base.RegisterServices();
        services.AddScoped<ITableCreationService, TableCreationService>();
    }

    [OneTimeSetUp]
    public override async Task OneTimeSetUp()
    {
        await base.OneTimeSetUp();
        
        _testSqlDirectory = Path.Combine(Path.GetTempPath(), "table-creation-tests");
        Directory.CreateDirectory(_testSqlDirectory);
        
        await CreateTestSqlFiles();
    }

    [OneTimeTearDown]
    public override async Task OneTimeTearDown()
    {
        if (Directory.Exists(_testSqlDirectory))
        {
            Directory.Delete(_testSqlDirectory, true);
        }
        
        await base.OneTimeTearDown();
    }

    [Test]
    public async Task TableExistsAsync_ExistingTable_ShouldReturnTrue()
    {
        // Arrange
        var tableName = "UsedLubeSamples"; // This table exists in test setup

        // Act
        var result = await Service.TableExistsAsync(tableName);

        // Assert
        result.Should().BeTrue();
    }

    [Test]
    public async Task TableExistsAsync_NonExistentTable_ShouldReturnFalse()
    {
        // Arrange
        var tableName = "NonExistentTable";

        // Act
        var result = await Service.TableExistsAsync(tableName);

        // Assert
        result.Should().BeFalse();
    }

    [Test]
    public async Task ValidateSqlScriptAsync_ValidScript_ShouldReturnValid()
    {
        // Arrange
        var scriptPath = Path.Combine(_testSqlDirectory, "ValidTable.sql");

        // Act
        var result = await Service.ValidateSqlScriptAsync(scriptPath);

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeTrue();
        result.DetectedTables.Should().Contain("ValidTable");
        result.Errors.Should().BeEmpty();
    }

    [Test]
    public async Task ValidateSqlScriptAsync_NonExistentScript_ShouldReturnError()
    {
        // Arrange
        var scriptPath = Path.Combine(_testSqlDirectory, "NonExistent.sql");

        // Act
        var result = await Service.ValidateSqlScriptAsync(scriptPath);

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(1);
        result.Errors[0].ErrorType.Should().Be("FileNotFound");
    }

    [Test]
    public async Task ValidateSqlScriptAsync_ScriptWithWarnings_ShouldReturnWarnings()
    {
        // Arrange
        var scriptPath = Path.Combine(_testSqlDirectory, "ScriptWithWarnings.sql");

        // Act
        var result = await Service.ValidateSqlScriptAsync(scriptPath);

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeTrue(); // Valid but with warnings
        result.Warnings.Should().NotBeEmpty();
        result.Warnings.Should().Contain(w => w.WarningType == "DangerousOperation");
    }

    [Test]
    public async Task GetAvailableSchemaFilesAsync_ShouldReturnSqlFiles()
    {
        // Act
        var result = await Service.GetAvailableSchemaFilesAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().NotBeEmpty();
        result.Should().Contain("ValidTable.sql");
        result.Should().Contain("TableWithFK.sql");
    }

    [Test]
    public async Task ResolveDependenciesAsync_ShouldReturnDependencyMap()
    {
        // Act
        var result = await Service.ResolveDependenciesAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().ContainKey("ValidTable");
        result.Should().ContainKey("TableWithFK");
        
        // TableWithFK should depend on ValidTable
        result["TableWithFK"].Should().Contain("ValidTable");
        
        // ValidTable should have no dependencies
        result["ValidTable"].Should().BeEmpty();
    }

    [Test]
    public async Task GetTableCreationOrderAsync_ShouldReturnCorrectOrder()
    {
        // Act
        var result = await Service.GetTableCreationOrderAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().NotBeEmpty();
        
        // ValidTable should come before TableWithFK due to dependency
        var validTableIndex = result.IndexOf("ValidTable");
        var fkTableIndex = result.IndexOf("TableWithFK");
        
        if (validTableIndex >= 0 && fkTableIndex >= 0)
        {
            validTableIndex.Should().BeLessThan(fkTableIndex);
        }
    }

    [Test]
    public async Task BackupTableAsync_ExistingTable_ShouldCreateBackup()
    {
        // Arrange
        var tableName = "UsedLubeSamples";

        // Act
        var result = await Service.BackupTableAsync(tableName);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.TableName.Should().Be(tableName);
        result.BackupTableName.Should().StartWith($"{tableName}_backup_");
        result.BackedUpRows.Should().BeGreaterOrEqualTo(0);
    }

    [Test]
    public async Task BackupTableAsync_NonExistentTable_ShouldFail()
    {
        // Arrange
        var tableName = "NonExistentTable";

        // Act
        var result = await Service.BackupTableAsync(tableName);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().NotBeNullOrEmpty();
    }

    [Test]
    public async Task ResolveTableConflictAsync_SkipStrategy_ShouldSkip()
    {
        // Arrange
        var tableName = "UsedLubeSamples"; // Existing table
        var strategy = ConflictResolutionStrategy.Skip;

        // Act
        var result = await Service.ResolveTableConflictAsync(tableName, strategy);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Action.Should().Be("Skipped");
        result.Strategy.Should().Be(strategy);
    }

    [Test]
    public async Task ResolveTableConflictAsync_BackupAndRecreateStrategy_ShouldBackup()
    {
        // Arrange
        var tableName = "UsedLubeSamples"; // Existing table
        var strategy = ConflictResolutionStrategy.BackupAndRecreate;

        // Act
        var result = await Service.ResolveTableConflictAsync(tableName, strategy);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Action.Should().Be("BackedUpAndWillRecreate");
        result.BackupResult.Should().NotBeNull();
        result.BackupResult!.Success.Should().BeTrue();
    }

    [Test]
    public async Task ResolveTableConflictAsync_NoConflict_ShouldReturnNoConflict()
    {
        // Arrange
        var tableName = "NonExistentTable";
        var strategy = ConflictResolutionStrategy.Skip;

        // Act
        var result = await Service.ResolveTableConflictAsync(tableName, strategy);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Action.Should().Be("NoConflict");
    }

    private async Task CreateTestSqlFiles()
    {
        // Valid table creation script
        var validTableSql = @"
CREATE TABLE [ValidTable] (
    [ID] int NOT NULL PRIMARY KEY,
    [Name] nvarchar(50) NOT NULL,
    [CreatedDate] datetime2 DEFAULT GETDATE()
);";
        await File.WriteAllTextAsync(Path.Combine(_testSqlDirectory, "ValidTable.sql"), validTableSql);

        // Table with foreign key dependency
        var tableWithFKSql = @"
CREATE TABLE [TableWithFK] (
    [ID] int NOT NULL PRIMARY KEY,
    [ValidTableID] int NOT NULL,
    [Description] nvarchar(100) NULL,
    FOREIGN KEY (ValidTableID) REFERENCES ValidTable(ID)
);";
        await File.WriteAllTextAsync(Path.Combine(_testSqlDirectory, "TableWithFK.sql"), tableWithFKSql);

        // Script with warnings
        var scriptWithWarningsSql = @"
-- This script contains potentially dangerous operations
USE [SomeDatabase]
GO

CREATE TABLE [TableWithWarnings] (
    [ID] int NOT NULL,
    [Data] nvarchar(max) NULL
);

-- This would be flagged as dangerous
DROP DATABASE SomeOtherDatabase;
";
        await File.WriteAllTextAsync(Path.Combine(_testSqlDirectory, "ScriptWithWarnings.sql"), scriptWithWarningsSql);

        // Invalid script (syntax error)
        var invalidSql = @"
CREATE TABLE [InvalidTable] (
    [ID] int NOT NULL,
    [Name] nvarchar(50) NOT NULL,
    -- Missing closing parenthesis
;";
        await File.WriteAllTextAsync(Path.Combine(_testSqlDirectory, "InvalidTable.sql"), invalidSql);
    }
}