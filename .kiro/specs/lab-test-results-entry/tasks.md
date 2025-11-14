# Implementation Plan

- [x] 1. Set up project structure and core infrastructure
  - Create Angular 20 project with Angular Material
  - Set up .NET 8 Web API project with minimal APIs
  - Configure Entity Framework Core with keyless entity support
  - Establish database connectivity and test basic operations
  - _Requirements: 1.1, 1.2, 10.1, 10.2, 10.3_

- [x] 1.1 Initialize Angular 20 project
  - Create new Angular project with latest CLI
  - Install Angular Material and configure theme
  - Set up project structure with feature modules
  - Configure TypeScript strict mode and linting
  - _Requirements: 1.1, 1.2_

- [x] 1.2 Configure Angular Material and base components
  - Set up Material theme and typography
  - Create shared component library (buttons, inputs, dialogs)
  - Implement responsive layout structure
  - Set up Angular routing configuration
  - _Requirements: 1.1, 1.2_

- [x] 1.3 Create .NET 8 Web API project
  - Initialize minimal API project structure
  - Configure dependency injection container
  - Set up NLog logging configuration
  - Create endpoint organization structure
  - _Requirements: 10.1, 10.2_

- [x] 1.4 Configure Entity Framework Core with keyless entities
  - Set up DbContext with existing database schema
  - Configure keyless entities for tables without primary keys
  - Create raw SQL service for keyless entity operations
  - Test database connectivity and basic CRUD operations
  - _Requirements: 10.2, 10.3, 10.4_

- [x] 2. Implement core sample management functionality
  - Create sample selection components and services
  - Implement sample data display templates
  - Build common navigation menu functionality
  - Add basic validation and error handling
  - _Requirements: 1.1, 1.2, 1.3, 8.1_

- [x] 2.1 Create sample selection service and API endpoints
  - Implement SampleEndpoints.cs with minimal API
  - Create SampleService for business logic
  - Build sample DTOs and AutoMapper profiles
  - Add sample filtering and search capabilities
  - _Requirements: 1.1, 1.2_

- [x] 2.2 Build sample selection Angular components
  - Create sample selection dropdown component using Angular Signals
  - Implement sample information display component
  - Add sample data template with reactive forms
  - Use new Angular control flow syntax (@if, @for)
  - _Requirements: 1.1, 1.2, 1.3_

- [x] 2.3 Implement common navigation menu
  - Create reusable navigation component
  - Add save, clear, delete functionality
  - Implement confirmation dialogs for destructive actions
  - Add keyboard shortcuts and accessibility features
  - _Requirements: 1.3, 2.4_

- [x] 3. Build basic test entry functionality for simple tests
  - Implement TAN by Color Indication test entry
  - Create Water-KF test entry form
  - Build TBN by Auto Titration test entry
  - Add trial-based data entry with validation
  - _Requirements: 2.1, 2.2, 2.3, 3.1, 3.2_

- [x] 3.1 Create test template infrastructure
  - Build TestEndpoints.cs with minimal API structure
  - Implement TestResultService with raw SQL operations
  - Create dynamic test template generation system
  - Add test field validation and calculation engine
  - _Requirements: 2.1, 2.2, 3.1, 3.2_

- [x] 3.2 Implement TAN test entry component
  - Create TAN-specific form with trial entries
  - Add sample weight and final buret input fields
  - Implement TAN calculation: (Final Buret * 5.61) / Sample Weight
  - Add validation for minimum value (0.01) and rounding to 2 decimal places
  - _Requirements: 2.1, 2.2, 3.1, 3.2, 3.5_

- [x] 3.3 Build Water-KF test entry component
  - Create Karl Fischer test form with result fields
  - Add file upload functionality for instrument data
  - Implement trial-based data entry with 4 trials
  - Add numeric validation and error messaging
  - _Requirements: 2.1, 2.2, 5.1, 5.2, 8.1_

- [x] 3.4 Create TBN test entry component
  - Build TBN by Auto Titration form
  - Add result field with numeric validation
  - Implement trial selection and common navigation
  - Add historical results view integration
  - _Requirements: 2.1, 2.2, 6.1, 6.2_

- [x] 4. Implement MTE equipment integration
  - Create equipment selection dropdowns
  - Build equipment calibration value lookup
  - Add equipment validation and due date checking
  - Implement equipment-specific business rules
  - _Requirements: 4.1, 4.2, 4.3_

- [x] 4.1 Create equipment management API endpoints
  - Implement EquipmentEndpoints.cs with MTE queries
  - Build equipment service with calibration lookups
  - Add equipment validation and due date logic
  - Create equipment DTOs and mapping
  - _Requirements: 4.1, 4.2_

- [x] 4.2 Build equipment selection components
  - Create MTE equipment dropdown component
  - Add equipment calibration value display
  - Implement due date warnings and validation
  - Add equipment-specific filtering by test type
  - _Requirements: 4.1, 4.2, 4.3_

- [x] 5. Create calculation engine for derived values
  - Implement viscosity calculations with tube calibration
  - Build flash point temperature corrections
  - Add grease penetration and NLGI lookup calculations
  - Create grease dropping point calculations
  - _Requirements: 3.1, 3.2, 3.3, 3.4, 3.5, 9.1, 9.2_

- [x] 5.1 Build calculation service infrastructure
  - Create ICalculationService interface and implementation
  - Add calculation methods for each test type
  - Implement rounding and precision handling
  - Add calculation validation and error handling
  - _Requirements: 3.1, 3.2, 3.3, 3.4, 3.5_

- [x] 5.2 Implement viscosity test calculations
  - Create viscosity @ 40°C and @ 100°C test forms
  - Add thermometer, stopwatch, and tube ID selection
  - Implement cSt calculation: Stop watch time * Tube calibration value
  - Add repeatability validation for Q/QAG samples
  - _Requirements: 3.1, 3.2, 4.1, 4.2_

- [x] 5.3 Build flash point test calculations
  - Create flash point test entry form
  - Add barometer and thermometer MTE selection
  - Implement result calculation: Flash Point temp + (0.06 * (760 - pressure))
  - Add rounding to nearest 2°F and validation
  - _Requirements: 3.1, 3.2, 4.1, 4.2_

- [x] 6. Implement file upload and data import functionality
  - Create file upload service and API endpoints
  - Build file preview and selection components
  - Add support for instrument data files (.DAT, .TXT)
  - Implement file validation and error handling
  - _Requirements: 5.1, 5.2, 5.3, 5.4_

- [x] 6.1 Create file upload API infrastructure
  - Implement FileUploadEndpoints.cs with minimal API
  - Add file validation and security checks
  - Create file storage and retrieval services
  - Add file association with samples and trials
  - _Requirements: 5.1, 5.2, 5.3_

- [x] 6.2 Build file upload Angular components
  - Create file selection and upload component
  - Add file preview functionality
  - Implement drag-and-drop file upload
  - Add upload progress and error handling
  - _Requirements: 5.1, 5.2, 5.4_

- [x] 7. Create historical results viewing system
  - Build historical results API endpoints
  - Implement last 12 results display component
  - Add resizable and single-screen mode views
  - Create extended history access functionality
  - _Requirements: 6.1, 6.2, 6.3, 6.4, 6.5_

- [x] 7.1 Implement historical results API
  - Create endpoints for historical data retrieval
  - Add filtering and pagination for large datasets
  - Implement efficient queries for last 12 results
  - Add extended history access with date ranges
  - _Requirements: 6.1, 6.2, 6.5_

- [x] 7.2 Build historical results display components
  - Create historical results table component
  - Add resizable panel functionality
  - Implement single-screen mode toggle
  - Add date filtering and search capabilities
  - _Requirements: 6.1, 6.2, 6.3, 6.4_

- [x] 8. Implement emission spectroscopy test entry
  - Create large emission spectroscopy test form
  - Add all element fields (Na, Cr, Sn, Si, Mo, Ca, Al, Ba, Mg, Ni, Mn, Zn, P, Ag, Pb, H, B, Cu, Fe)
  - Implement file upload for spectroscopy data
  - Add Ferrography scheduling option
  - _Requirements: 2.1, 2.2, 5.1, 5.2_

- [x] 8.1 Create emission spectroscopy API endpoints
  - Implement specialized endpoints for EmSpectro table
  - Add raw SQL operations for keyless entity
  - Create element data validation and processing
  - Add Ferrography scheduling logic
  - _Requirements: 2.1, 2.2, 10.2, 10.3_

- [x] 8.2 Build emission spectroscopy form component
  - Create dynamic form with all element fields (Na, Cr, Sn, Si, Mo, Ca, Al, Ba, Mg, Ni, Mn, Zn, P, Ag, Pb, H, B, Cu, Fe)
  - Add file upload integration for instrument data
  - Implement trial-based entry with validation
  - Add Ferrography scheduling checkbox functionality in Trial 1
  - _Requirements: 2.1, 2.2, 5.1, 5.2_

- [x] 9. Create particle count test with NAS lookup
  - Build particle count test entry form
  - Implement NAS lookup calculation system
  - Add particle size range fields (5-10, 10-15, 15-25, 25-50, 50-100, >100)
  - Create lookup table integration
  - _Requirements: 2.1, 2.2, 9.1, 9.2, 9.3_

- [x] 9.1 Implement NAS lookup system
  - Create LookupEndpoints.cs with NAS calculation
  - Build NAS lookup table queries
  - Add highest associated NAS value determination
  - Implement lookup result caching
  - _Requirements: 9.1, 9.2_

- [x] 9.2 Build particle count entry component
  - Create particle count form with size range fields
  - Add file upload for particle counter data
  - Implement automatic NAS lookup on value change
  - Add validation for particle count ranges
  - _Requirements: 2.1, 2.2, 5.1, 9.1, 9.2_

- [x] 10. Implement grease testing functionality
  - Create grease penetration worked test entry
  - Build grease dropping point test form
  - Add NLGI lookup integration
  - Implement specialized grease calculations
  - _Requirements: 3.1, 3.2, 9.1, 9.2_

- [x] 10.1 Build grease penetration test component
  - Create form with 1st, 2nd, 3rd penetration fields
  - Implement result calculation: ((Average of 3 penetrations) * 3.75) + 24
  - Add NLGI lookup based on calculated result
  - Add validation and rounding to 0 decimal places
  - _Requirements: 3.1, 3.2, 9.1, 9.2_

- [x] 10.2 Create grease dropping point test component
  - Build form with thermometer MTE selections
  - Add dropping point and block temperature fields
  - Implement calculation: Dropping Point + ((Block Temp - Dropping Point) / 3)
  - Add thermometer validation to prevent same selection
  - _Requirements: 3.1, 3.2, 4.1, 4.2_

- [x] 11. Create specialized particle analysis tests
  - Implement Inspect Filter test with particle type characterization
  - Build Ferrography test with dilution factors
  - Add particle type evaluation system
  - Create overall severity calculation
  - _Requirements: 7.1, 7.2, 7.3, 7.4, 7.5_

- [x] 11.1 Build particle type infrastructure
  - Create ParticleAnalysisEndpoints.cs with minimal API
  - Implement particle type definition queries
  - Add particle sub-type category management
  - Create particle type image and description system
  - _Requirements: 7.1, 7.2_

- [x] 11.2 Implement Inspect Filter test component
  - Create particle type characterization interface
  - Add Heat, Concentration, Size, Color, Texture, Composition, Severity fields
  - Implement filtered views (All Records vs Review)
  - Add overall severity calculation and media ready functionality
  - _Requirements: 7.1, 7.2, 7.3, 7.4, 7.5_

- [x] 11.3 Build Ferrography test component
  - Create Ferrography-specific particle analysis form
  - Add dilution factor selection (3:2, 1:10, 1:100, X/YYYY)
  - Implement partial save functionality
  - Add test status management (X to E on dilution factor assignment)
  - _Requirements: 7.1, 7.2, 7.3, 7.4, 7.5_

- [x] 12. Implement remaining specialized tests
  - Create RBOT test entry form
  - Build Rust test with pass/fail options
  - Add TFOUT test entry
  - Implement Deleterious, D-inch, Oil Content, and Varnish Potential Rating tests
  - _Requirements: 2.1, 2.2, 4.1, 5.1_

- [x] 12.1 Build RBOT and TFOUT test components
  - Create RBOT form with thermometer MTE and fail time fields
  - Add file upload and preview for RBOT data files (.DAT format)
  - Implement TFOUT form with thermometer MTE and fail time
  - Add validation and file association for both tests
  - _Requirements: 2.1, 2.2, 4.1, 5.1_

- [x] 12.2 Create Rust test component
  - Build Rust test with thermometer MTE selection
  - Add Pass/Fail radio button options (Pass, Fail-Light, Fail-Moderate, Fail-Severe)
  - Implement trial-based entry with validation
  - Add consistent navigation and error handling
  - _Requirements: 2.1, 2.2, 4.1_

- [x] 12.3 Create remaining simple test components
  - Build Deleterious test with MTE selection, pressure, scratches, and pass/fail
  - Create D-inch test with simple numeric entry
  - Add Oil Content test with numeric entry
  - Implement Varnish Potential Rating test with numeric entry
  - _Requirements: 2.1, 2.2, 4.1_

- [x] 12.4 Evaluate Rheometer test requirements
  - Review existing Rheometer data conversion process (.TXT files)
  - Determine if manual test entry is needed or if automated conversion is sufficient
  - Document decision and implement if required
  - _Requirements: 2.1, 2.2_

- [x] 13. Add comprehensive validation and error handling
  - Implement client-side validation with Angular reactive forms
  - Create server-side validation with detailed error messages
  - Add field-specific validation rules per test type
  - Build user-friendly error display system
  - _Requirements: 8.1, 8.2, 8.3, 8.4_

- [x] 13.1 Create validation service infrastructure
  - Build ValidationService with test-specific rules
  - Implement field validation for numeric, required, and custom rules
  - Add validation error aggregation and display
  - Create validation state management with Angular Signals
  - _Requirements: 8.1, 8.2, 8.3_

- [x] 13.2 Implement comprehensive error handling
  - Add global error interceptor for HTTP errors
  - Create user-friendly error messages and notifications
  - Implement validation error highlighting in forms
  - Add error logging and debugging capabilities
  - _Requirements: 8.1, 8.2, 8.4_

- [x] 14. Create lookup table integration system
  - Implement NAS lookup table queries and caching
  - Build NLGI lookup system for grease tests
  - Add MTE equipment lookup with calibration values
  - Create comment and particle type lookup systems
  - _Requirements: 9.1, 9.2, 9.3, 4.1, 4.2_

- [x] 14.1 Build lookup service infrastructure
  - Create ILookupService interface and implementation
  - Add caching for frequently accessed lookup data
  - Implement efficient lookup queries with raw SQL
  - Add lookup data refresh and cache invalidation
  - _Requirements: 9.1, 9.2, 9.3_

- [x] 14.2 Integrate lookup functionality in components
  - Add automatic lookup triggers on field changes
  - Implement lookup result display and validation
  - Create lookup loading states and error handling
  - Add manual lookup refresh capabilities
  - _Requirements: 9.1, 9.2, 9.3_

- [x] 15. Implement user authentication and authorization
  - Create user qualification system (Q/QAG, TRAIN, MicrE)
  - Add role-based access control for test entry and review
  - Implement audit trail for all data operations
  - Build user session management
  - _Requirements: 1.1, 1.2, 2.4, 10.4_

- [x] 15.1 Build authentication API infrastructure
  - Create user authentication endpoints
  - Implement JWT token generation and validation
  - Add role-based authorization middleware
  - Create user qualification checking system
  - _Requirements: 1.1, 1.2, 2.4_

- [x] 15.2 Implement frontend authentication
  - Create login/logout components
  - Add JWT token management and storage
  - Implement route guards for protected areas
  - Add user role-based UI element visibility
  - _Requirements: 1.1, 1.2, 2.4_

- [x] 16. Add comprehensive testing suite
  - Create unit tests for all services and components
  - Build integration tests for API endpoints
  - Add end-to-end tests for critical user workflows
  - Implement test data setup and teardown
  - _Requirements: All requirements validation_

- [x] 16.1 Create frontend unit tests
  - Write component tests using Angular Testing Utilities
  - Add service tests with mocked dependencies
  - Test calculation logic and validation rules
  - Create form validation and user interaction tests
  - _Requirements: All frontend requirements_

- [x] 16.2 Build backend unit and integration tests
  - Write service layer unit tests
  - Add repository and raw SQL operation tests
  - Create API endpoint integration tests
  - Test calculation and validation logic
  - _Requirements: All backend requirements_

- [x] 16.3 Implement end-to-end testing
  - Create E2E tests for complete user workflows
  - Add tests for each test type entry process
  - Test file upload and data import functionality
  - Validate cross-browser compatibility
  - _Requirements: All user workflow requirements_

- [x] 17. Performance optimization and deployment preparation
  - Optimize Angular application for production
  - Implement API response caching and optimization
  - Add database query optimization
  - Prepare deployment configurations
  - _Requirements: 10.1, 10.2, 10.3, 10.4_

- [x] 17.1 Fix API compilation issues and complete integration
  - Resolve model property mismatches between DTOs and test files
  - Complete missing DTO implementations referenced in endpoints
  - Fix interface implementation issues in services
  - Resolve DbContext configuration issues with keyless entities
  - _Requirements: 10.2, 10.3_

- [x] 17.2 Frontend performance optimization
  - Implement lazy loading for test modules (partially done)
  - Add OnPush change detection strategy to components
  - Optimize bundle size and implement code splitting
  - Add service worker for caching
  - _Requirements: 10.1_

- [x] 17.3 Backend performance optimization
  - Optimize database queries and add indexing recommendations
  - Implement response caching for lookup data (partially implemented)
  - Add connection pooling and async patterns
  - Create performance monitoring and logging
  - _Requirements: 10.2, 10.3_

- [x] 17.4 Complete testing suite integration
  - Integrate existing unit tests with main API project
  - Add missing integration tests for all endpoints
  - Complete E2E test coverage for all test types
  - Add test data setup and teardown automation
  - _Requirements: All requirements validation_

- [x] 17.5 Deployment configuration
  - Create production build configurations
  - Set up IIS deployment for Angular and API
  - Configure database connection strings and security
  - Add monitoring and health check endpoints (partially done)
  - _Requirements: 10.4_

- [-] 18. Critical bug fixes and system stabilization
  - Fix API compilation errors preventing system startup
  - Resolve frontend-backend integration issues
  - Complete missing service implementations
  - Fix authentication and authorization flow
  - _Requirements: 1.1, 1.2, 10.1, 10.2_

- [x] 18.1 API compilation and startup fixes
  - Fix DTO property mismatches causing compilation errors
  - Complete missing service interface implementations
  - Resolve Entity Framework configuration issues
  - Fix dependency injection registration issues
  - _Requirements: 10.1, 10.2_

- [x] 18.2 Frontend-backend integration fixes
  - Fix HTTP service calls to match actual API endpoints
  - Resolve authentication token handling issues
  - Fix data model mismatches between frontend and backend
  - Complete error handling and validation integration
  - _Requirements: 1.1, 1.2, 8.1, 8.2_

- [x] 18.3 Database integration stabilization
  - Fix raw SQL service implementations for keyless entities
  - Complete missing stored procedure and function integrations
  - Resolve connection string and database access issues
  - Add proper transaction handling for complex operations
  - _Requirements: 10.2, 10.3, 10.4_

- [x] 18.4 Authentication and authorization completion
  - Fix JWT token generation and validation
  - Complete user qualification checking system
  - Implement proper role-based access control
  - Add audit trail functionality for all operations
  - _Requirements: 1.1, 1.2, 2.4, 10.4_