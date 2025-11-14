using NUnit.Framework;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using LabResultsApi.Data;
using LabResultsApi.Services;
using LabResultsApi.DTOs;
using LabResultsApi.Models;

namespace LabResultsApi.Tests.Services;

[TestFixture]
public class AuthenticationServiceTests
{
    private LabDbContext _context;
    private AuthenticationService _authService;
    private IConfiguration _configuration;
    private ILogger<AuthenticationService> _logger;

    [SetUp]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<LabDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new LabDbContext(options);
        
        var configData = new Dictionary<string, string>
        {
            {"Jwt:Key", "test-key-that-is-at-least-32-characters-long-for-security"},
            {"Jwt:Issuer", "TestIssuer"},
            {"Jwt:Audience", "TestAudience"},
            {"Jwt:ExpirationHours", "8"}
        };
        
        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configData!)
            .Build();
            
        _logger = new LoggerFactory().CreateLogger<AuthenticationService>();
        _authService = new AuthenticationService(_context, _configuration, _logger);
        
        SeedTestData();
    }

    [TearDown]
    public void TearDown()
    {
        _context.Dispose();
    }

    private void SeedTestData()
    {
        // Add test LubeTech
        var lubeTech = new LubeTech
        {
            EmployeeId = "12345",
            FirstName = "John",
            LastName = "Doe",
            MiddleInitial = "A",
            QualificationPassword = "password"
        };
        _context.LubeTechs.Add(lubeTech);

        // Add test Reviewer
        var reviewer = new Reviewer
        {
            EmployeeId = "67890",
            FirstName = "Jane",
            LastName = "Smith",
            MiddleInitial = "B",
            ReviewerPassword = "password",
            Level = 2
        };
        _context.Reviewers.Add(reviewer);

        _context.SaveChanges();
    }

    [Test]
    public async Task AuthenticateAsync_ValidLubeTech_ReturnsLoginResponse()
    {
        // Arrange
        var loginRequest = new LoginRequestDto
        {
            EmployeeId = "12345",
            Password = "password"
        };

        // Act
        var result = await _authService.AuthenticateAsync(loginRequest);

        // Assert
        result.Should().NotBeNull();
        result!.EmployeeId.Should().Be("12345");
        result.Role.Should().Be("Technician");
        result.FullName.Should().Be("John A Doe");
        result.Token.Should().NotBeEmpty();
    }

    [Test]
    public async Task AuthenticateAsync_ValidReviewer_ReturnsLoginResponse()
    {
        // Arrange
        var loginRequest = new LoginRequestDto
        {
            EmployeeId = "67890",
            Password = "password"
        };

        // Act
        var result = await _authService.AuthenticateAsync(loginRequest);

        // Assert
        result.Should().NotBeNull();
        result!.EmployeeId.Should().Be("67890");
        result.Role.Should().Be("Reviewer");
        result.FullName.Should().Be("Jane B Smith");
        result.Token.Should().NotBeEmpty();
    }

    [Test]
    public async Task AuthenticateAsync_InvalidCredentials_ReturnsNull()
    {
        // Arrange
        var loginRequest = new LoginRequestDto
        {
            EmployeeId = "12345",
            Password = "wrongpassword"
        };

        // Act
        var result = await _authService.AuthenticateAsync(loginRequest);

        // Assert
        result.Should().BeNull();
    }

    [Test]
    public async Task GetUserInfoAsync_ValidEmployeeId_ReturnsUserInfo()
    {
        // Arrange
        var employeeId = "12345";

        // Act
        var result = await _authService.GetUserInfoAsync(employeeId);

        // Assert
        result.Should().NotBeNull();
        result!.EmployeeId.Should().Be("12345");
        result.Role.Should().Be("Technician");
        result.FullName.Should().Be("John A Doe");
    }

    [Test]
    public void ValidateToken_ValidToken_ReturnsTrue()
    {
        // Arrange
        var lubeTech = new LubeTech
        {
            EmployeeId = "12345",
            FirstName = "John",
            LastName = "Doe"
        };
        var qualifications = new List<LubeTechQualification>();
        var token = _authService.GenerateJwtToken(lubeTech, qualifications);

        // Act
        var result = _authService.ValidateToken(token);

        // Assert
        result.Should().BeTrue();
    }

    [Test]
    public void ValidateToken_InvalidToken_ReturnsFalse()
    {
        // Arrange
        var invalidToken = "invalid.token.here";

        // Act
        var result = _authService.ValidateToken(invalidToken);

        // Assert
        result.Should().BeFalse();
    }

    [Test]
    public void GetEmployeeIdFromToken_ValidToken_ReturnsEmployeeId()
    {
        // Arrange
        var lubeTech = new LubeTech
        {
            EmployeeId = "12345",
            FirstName = "John",
            LastName = "Doe"
        };
        var qualifications = new List<LubeTechQualification>();
        var token = _authService.GenerateJwtToken(lubeTech, qualifications);

        // Act
        var result = _authService.GetEmployeeIdFromToken(token);

        // Assert
        result.Should().Be("12345");
    }

    [Test]
    public void GetEmployeeIdFromToken_InvalidToken_ReturnsNull()
    {
        // Arrange
        var invalidToken = "invalid.token.here";

        // Act
        var result = _authService.GetEmployeeIdFromToken(invalidToken);

        // Assert
        result.Should().BeNull();
    }
}