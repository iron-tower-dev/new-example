# Laboratory Test Results Entry System Design

## Overview

This document outlines the design for modernizing the Laboratory Test Results Entry System from VB ASP.NET to Angular 20 frontend with .NET 8 Web API backend. The system will maintain all existing functionality while providing a modern, responsive user interface and robust API architecture.

## Architecture

### High-Level Architecture

```
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│   Angular 20    │    │   .NET 8 Web    │    │   SQL Server    │
│   Frontend      │◄──►│      API        │◄──►│   Database      │
│                 │    │                 │    │                 │
└─────────────────┘    └─────────────────┘    └─────────────────┘
```

### Technology Stack

**Frontend:**
- Angular 20 with TypeScript
- Angular Material for UI components
- Angular Signals for reactive state management
- RxJS for HTTP operations and complex async flows
- Angular Forms (Reactive Forms)
- Angular Router for navigation
- New Angular control flow syntax (@if, @for, @switch)

**Backend:**
- .NET 8 Web API with Minimal APIs organized by feature
- Entity Framework Core for data access with keyless entities
- Raw SQL execution for tables without primary keys
- NLog for logging
- AutoMapper for object mapping
- Separate endpoint files for organization

**Database:**
- Existing SQL Server database
- Maintain current schema and relationships
- Support for tables without primary keys

## Components and Interfaces

### Frontend Components

#### 1. Sample Selection Component
**Purpose:** Allow technicians to select samples for test entry
**Features:**
- Dropdown list of available samples by test type
- Sample information display
- Navigation to test entry forms

#### 2. Test Template Components
**Purpose:** Dynamic test entry forms based on test type
**Features:**
- Trial-based data entry (up to 4 trials)
- Field validation and calculations
- MTE equipment selection
- File upload capabilities

#### 3. Historical Results Component
**Purpose:** Display last 12 test results for reference
**Features:**
- Resizable view section
- Single screen mode toggle
- Extended history access

#### 4. Particle Analysis Components
**Purpose:** Specialized components for Inspect Filter and Ferrography tests
**Features:**
- Particle type characterization
- Image display and references
- Overall severity calculation
- Filtered views (All/Review)

### Angular Signals Implementation

**State Management with Signals**
```typescript
@Injectable()
export class TestResultsService {
  // Signals for reactive state management
  private _selectedSample = signal<Sample | null>(null);
  private _testResults = signal<TestResult[]>([]);
  private _isLoading = signal(false);
  private _validationErrors = signal<ValidationError[]>([]);
  
  // Computed signals
  readonly selectedSample = this._selectedSample.asReadonly();
  readonly testResults = this._testResults.asReadonly();
  readonly isLoading = this._isLoading.asReadonly();
  readonly hasValidationErrors = computed(() => this._validationErrors().length > 0);
  
  // Effects for side effects
  constructor() {
    effect(() => {
      const sample = this._selectedSample();
      if (sample) {
        this.loadTestResults(sample.id);
      }
    });
  }
}
```

**Component with New Control Flow**
```typescript
@Component({
  template: `
    <div class="test-entry-container">
      @if (isLoading()) {
        <mat-spinner></mat-spinner>
      } @else {
        <div class="sample-info">
          @if (selectedSample(); as sample) {
            <h3>Sample: {{ sample.id }}</h3>
            <p>Equipment: {{ sample.tagNumber }}</p>
          }
        </div>
        
        <div class="trials-container">
          @for (trial of trials(); track trial.number) {
            <div class="trial-row">
              <span>Trial {{ trial.number }}</span>
              @switch (testType()) {
                @case ('TAN') {
                  <app-tan-trial [trial]="trial" (valueChange)="onTrialChange($event)"/>
                }
                @case ('Viscosity') {
                  <app-viscosity-trial [trial]="trial" (valueChange)="onTrialChange($event)"/>
                }
                @default {
                  <app-generic-trial [trial]="trial" (valueChange)="onTrialChange($event)"/>
                }
              }
            </div>
          }
        </div>
        
        @if (hasValidationErrors()) {
          <div class="validation-errors">
            @for (error of validationErrors(); track error.field) {
              <mat-error>{{ error.message }}</mat-error>
            }
          </div>
        }
      }
    </div>
  `
})
export class TestEntryComponent {
  private testService = inject(TestResultsService);
  
  // Signals from service
  readonly selectedSample = this.testService.selectedSample;
  readonly isLoading = this.testService.isLoading;
  readonly hasValidationErrors = this.testService.hasValidationErrors;
  readonly validationErrors = this.testService.validationErrors;
  
  // Local signals
  readonly testType = signal<string>('');
  readonly trials = signal<Trial[]>([]);
  
  onTrialChange(event: TrialChangeEvent) {
    // Update trial data and trigger calculations
    this.updateTrial(event.trialNumber, event.field, event.value);
  }
}

### Backend API Structure

#### 1. Minimal API Endpoints (Organized by Feature)

**SampleEndpoints.cs**
```csharp
public static class SampleEndpoints
{
    public static void MapSampleEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/samples");
        
        group.MapGet("/by-test/{testId}", GetSamplesByTest);
        group.MapGet("/{sampleId}", GetSample);
        group.MapGet("/{sampleId}/history/{testId}", GetSampleHistory);
    }
}
```

**TestEndpoints.cs**
```csharp
public static class TestEndpoints
{
    public static void MapTestEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/tests");
        
        group.MapGet("/", GetTests);
        group.MapGet("/{testId}/template", GetTestTemplate);
        group.MapPost("/{testId}/results", SaveTestResults);
        group.MapPut("/{testId}/results/{sampleId}", UpdateTestResults);
        group.MapDelete("/{testId}/results/{sampleId}", DeleteTestResults);
    }
}
```

**EquipmentEndpoints.cs**
```csharp
public static class EquipmentEndpoints
{
    public static void MapEquipmentEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/equipment");
        
        group.MapGet("/mte/{equipmentType}", GetMTEEquipment);
        group.MapGet("/calibration/{equipmentId}", GetCalibrationValue);
    }
}
```

**LookupEndpoints.cs**
```csharp
public static class LookupEndpoints
{
    public static void MapLookupEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/lookups");
        
        group.MapGet("/nas", GetNASLookup);
        group.MapGet("/nlgi/{penetrationValue}", GetNLGILookup);
        group.MapGet("/particle-types", GetParticleTypes);
    }
}
```

#### 2. Data Transfer Objects (DTOs)

**SampleDto**
```csharp
public class SampleDto
{
    public int Id { get; set; }
    public string TagNumber { get; set; }
    public string Component { get; set; }
    public string Location { get; set; }
    public string LubeType { get; set; }
    public string QualityClass { get; set; }
    public DateTime SampleDate { get; set; }
    public string Status { get; set; }
}
```

**TestResultDto**
```csharp
public class TestResultDto
{
    public int SampleId { get; set; }
    public int TestId { get; set; }
    public int TrialNumber { get; set; }
    public Dictionary<string, object> Values { get; set; }
    public string Status { get; set; }
    public DateTime EntryDate { get; set; }
    public string EntryId { get; set; }
}
```

**TestTemplateDto**
```csharp
public class TestTemplateDto
{
    public int TestId { get; set; }
    public string TestName { get; set; }
    public List<TestFieldDto> Fields { get; set; }
    public int MaxTrials { get; set; }
    public bool RequiresCalculation { get; set; }
    public bool SupportsFileUpload { get; set; }
}
```

## Data Models

### Entity Framework Models

#### 1. Core Entities

**Sample Entity (Has Primary Key)**
```csharp
public class Sample
{
    public int Id { get; set; }
    public string TagNumber { get; set; }
    public string Component { get; set; }
    public string Location { get; set; }
    public string LubeType { get; set; }
    public DateTime SampleDate { get; set; }
    public int Status { get; set; }
    // Note: No navigation properties due to keyless related entities
}
```

**TestReading Entity (Keyless - requires raw SQL)**
```csharp
[Keyless]
public class TestReading
{
    public int SampleId { get; set; }
    public int TestId { get; set; }
    public int TrialNumber { get; set; }
    public double? Value1 { get; set; }
    public double? Value2 { get; set; }
    public double? Value3 { get; set; }
    public double? TrialCalc { get; set; }
    public string Id1 { get; set; }
    public string Id2 { get; set; }
    public string Id3 { get; set; }
    public string Status { get; set; }
    public DateTime? EntryDate { get; set; }
    public string MainComments { get; set; }
}
```

#### 2. Specialized Test Entities

**EmissionSpectroscopy Entity (Keyless - requires raw SQL)**
```csharp
[Keyless]
public class EmissionSpectroscopy
{
    public int Id { get; set; }
    public int TestId { get; set; }
    public int TrialNum { get; set; }
    public double? Na { get; set; }
    public double? Mo { get; set; }
    public double? Mg { get; set; }
    // ... other elements
    public DateTime? TrialDate { get; set; }
}
```

**ParticleType Entity (Composite Key)**
```csharp
public class ParticleType
{
    public int SampleId { get; set; }
    public int TestId { get; set; }
    public int ParticleTypeDefinitionId { get; set; }
    public string Status { get; set; }
    public string Comments { get; set; }
    // Note: Limited navigation properties due to keyless related entities
    // Will use raw SQL for complex joins
}
```

### Database Context Configuration

```csharp
public class LabDbContext : DbContext
{
    public DbSet<Sample> UsedLubeSamples { get; set; }
    public DbSet<TestReading> TestReadings { get; set; }
    public DbSet<Test> Tests { get; set; }
    public DbSet<EmissionSpectroscopy> EmSpectro { get; set; }
    public DbSet<ParticleType> ParticleTypes { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configure keyless entities (tables without primary keys)
        modelBuilder.Entity<TestReading>().HasNoKey().ToTable("TestReadings");
        modelBuilder.Entity<EmissionSpectroscopy>().HasNoKey().ToTable("EmSpectro");
        
        // Configure composite keys where applicable
        modelBuilder.Entity<ParticleType>()
            .HasKey(p => new { p.SampleId, p.TestId, p.ParticleTypeDefinitionId });
            
        // Map to existing table names
        modelBuilder.Entity<Sample>().ToTable("UsedLubeSamples");
        
        // Note: Many operations will require raw SQL due to keyless entities
        // and complex relationships that EF Core cannot track
    }
    
    // Raw SQL execution methods for keyless entities
    public async Task<List<TestReading>> GetTestReadingsAsync(int sampleId, int testId)
    {
        return await TestReadings
            .FromSqlRaw("SELECT * FROM TestReadings WHERE sampleID = {0} AND testID = {1}", 
                       sampleId, testId)
            .ToListAsync();
    }
    
    public async Task<int> SaveTestReadingAsync(TestReading reading)
    {
        return await Database.ExecuteSqlRawAsync(
            @"INSERT INTO TestReadings (sampleID, testID, trialNumber, value1, value2, value3, 
              trialCalc, ID1, ID2, ID3, status, entryDate, MainComments) 
              VALUES ({0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10}, {11}, {12})",
            reading.SampleId, reading.TestId, reading.TrialNumber, reading.Value1, 
            reading.Value2, reading.Value3, reading.TrialCalc, reading.Id1, 
            reading.Id2, reading.Id3, reading.Status, reading.EntryDate, reading.MainComments);
    }
}
```

## Error Handling

### Frontend Error Handling

**Global Error Interceptor**
```typescript
@Injectable()
export class ErrorInterceptor implements HttpInterceptor {
  intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    return next.handle(req).pipe(
      catchError((error: HttpErrorResponse) => {
        if (error.status === 400) {
          // Handle validation errors
          this.showValidationErrors(error.error);
        } else if (error.status === 500) {
          // Handle server errors
          this.showServerError();
        }
        return throwError(error);
      })
    );
  }
}
```

**Field Validation Service**
```typescript
@Injectable()
export class ValidationService {
  validateNumericField(value: any): ValidationErrors | null {
    if (value && isNaN(value)) {
      return { numeric: true };
    }
    return null;
  }
  
  validateRequiredField(value: any): ValidationErrors | null {
    if (!value || value.toString().trim().length === 0) {
      return { required: true };
    }
    return null;
  }
}
```

### Backend Error Handling

**Global Exception Handler**
```csharp
public class GlobalExceptionMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (ValidationException ex)
        {
            await HandleValidationException(context, ex);
        }
        catch (Exception ex)
        {
            await HandleGenericException(context, ex);
        }
    }
}
```

## Testing Strategy

### Frontend Testing

**Unit Tests**
- Component testing with Angular Testing Utilities
- Service testing with Jasmine/Karma
- Form validation testing
- Calculation logic testing

**Integration Tests**
- API integration testing
- End-to-end user workflows
- File upload functionality

**Test Structure Example**
```typescript
describe('TanTestComponent', () => {
  let component: TanTestComponent;
  let fixture: ComponentFixture<TanTestComponent>;
  
  beforeEach(() => {
    TestBed.configureTestingModule({
      declarations: [TanTestComponent],
      imports: [ReactiveFormsModule, MaterialModule]
    });
  });
  
  it('should calculate TAN correctly', () => {
    component.sampleWeight = 10;
    component.finalBuret = 5;
    component.calculateTAN();
    expect(component.tanResult).toBe(2.81);
  });
});
```

### Backend Testing

**Unit Tests**
- Service layer testing
- Repository pattern testing
- Calculation logic testing
- Validation testing

**Integration Tests**
- Database integration testing
- API endpoint testing
- Raw SQL execution testing

**Test Structure Example**
```csharp
[Test]
public async Task CalculateTAN_ValidInputs_ReturnsCorrectResult()
{
    // Arrange
    var service = new CalculationService();
    var sampleWeight = 10.0;
    var finalBuret = 5.0;
    
    // Act
    var result = service.CalculateTAN(sampleWeight, finalBuret);
    
    // Assert
    Assert.AreEqual(2.81, result, 0.01);
}
```

## Security Considerations

### Authentication & Authorization
- Implement role-based access control
- Maintain existing user qualification system (Q/QAG, TRAIN, MicrE)
- Secure API endpoints with JWT tokens

### Data Validation
- Server-side validation for all inputs
- SQL injection prevention through parameterized queries
- File upload security with type and size restrictions

### Audit Trail
- Maintain existing audit functionality
- Log all data entry and modification operations
- Track user actions and timestamps

## Performance Optimization

### Frontend Optimization
- Lazy loading for test modules
- OnPush change detection strategy
- Virtual scrolling for large datasets
- Caching of lookup data

### Backend Optimization
- Database connection pooling
- Async/await patterns throughout
- Efficient raw SQL queries for complex operations
- Response caching for static data

## Migration Strategy

### Phase 1: Core Infrastructure
- Set up Angular 20 project structure
- Implement .NET 8 Web API foundation
- Configure Entity Framework Core
- Establish database connectivity

### Phase 2: Basic Test Entry
- Implement simple test types (TAN, Water-KF, TBN)
- Basic validation and calculations
- Sample selection functionality

### Phase 3: Complex Test Types
- Emission Spectroscopy implementation
- Viscosity tests with MTE integration
- Particle Count with NAS lookup

### Phase 4: Specialized Tests
- Inspect Filter implementation
- Ferrography with particle analysis
- File upload functionality

### Phase 5: Advanced Features
- Historical data views
- Reporting capabilities
- Performance optimization

## API Organization Structure

### Minimal API File Organization
```
/Endpoints/
├── SampleEndpoints.cs
├── TestEndpoints.cs
├── EquipmentEndpoints.cs
├── LookupEndpoints.cs
├── ParticleAnalysisEndpoints.cs
└── FileUploadEndpoints.cs

/Services/
├── ISampleService.cs
├── SampleService.cs
├── ITestResultService.cs
├── TestResultService.cs
├── ICalculationService.cs
├── CalculationService.cs
└── IRawSqlService.cs
└── RawSqlService.cs
```

### Program.cs Configuration
```csharp
var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddDbContext<LabDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<ISampleService, SampleService>();
builder.Services.AddScoped<ITestResultService, TestResultService>();
builder.Services.AddScoped<IRawSqlService, RawSqlService>();

var app = builder.Build();

// Map all endpoint groups
app.MapSampleEndpoints();
app.MapTestEndpoints();
app.MapEquipmentEndpoints();
app.MapLookupEndpoints();
app.MapParticleAnalysisEndpoints();
app.MapFileUploadEndpoints();

app.Run();
```

### Raw SQL Service for Keyless Entities
```csharp
public interface IRawSqlService
{
    Task<List<TestReading>> GetTestReadingsAsync(int sampleId, int testId);
    Task<int> SaveTestReadingAsync(TestReading reading);
    Task<int> UpdateTestReadingAsync(TestReading reading);
    Task<int> DeleteTestReadingsAsync(int sampleId, int testId);
    Task<List<EmissionSpectroscopy>> GetEmissionSpectroscopyAsync(int sampleId, int testId);
    Task<int> SaveEmissionSpectroscopyAsync(EmissionSpectroscopy data);
}

public class RawSqlService : IRawSqlService
{
    private readonly LabDbContext _context;
    
    public RawSqlService(LabDbContext context)
    {
        _context = context;
    }
    
    public async Task<List<TestReading>> GetTestReadingsAsync(int sampleId, int testId)
    {
        return await _context.TestReadings
            .FromSqlRaw(@"
                SELECT sampleID, testID, trialNumber, value1, value2, value3, 
                       trialCalc, ID1, ID2, ID3, trialComplete, status, 
                       schedType, entryID, validateID, entryDate, valiDate, MainComments
                FROM TestReadings 
                WHERE sampleID = {0} AND testID = {1} 
                ORDER BY trialNumber", sampleId, testId)
            .ToListAsync();
    }
    
    public async Task<int> SaveTestReadingAsync(TestReading reading)
    {
        return await _context.Database.ExecuteSqlRawAsync(@"
            INSERT INTO TestReadings 
            (sampleID, testID, trialNumber, value1, value2, value3, trialCalc, 
             ID1, ID2, ID3, status, entryDate, MainComments, entryID)
            VALUES ({0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10}, {11}, {12}, {13})",
            reading.SampleId, reading.TestId, reading.TrialNumber, 
            reading.Value1, reading.Value2, reading.Value3, reading.TrialCalc,
            reading.Id1, reading.Id2, reading.Id3, reading.Status, 
            reading.EntryDate, reading.MainComments, reading.EntryId);
    }
}

## Deployment Architecture

### Development Environment
- Local Angular development server
- Local .NET 8 API with IIS Express
- SQL Server LocalDB or development instance

### Production Environment
- Angular application served by IIS
- .NET 8 API hosted on IIS
- SQL Server production database
- Load balancing considerations for high availability