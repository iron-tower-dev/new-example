# Laboratory Test Results Entry System Requirements

## Introduction

This document specifies the requirements for modernizing the Laboratory Test Results Entry System from VB ASP.NET to a modern Angular 20 frontend with .NET 8 Web API backend. The system enables laboratory technicians to enter, validate, and manage test results for various types of laboratory tests including TAN, Water-KF, TBN, Emission Spectroscopy, Viscosity, Flashpoint, Particle Count, and specialized tests like Inspect Filter and Ferrography.

## Glossary

- **Lab_System**: The modernized Laboratory Test Results Entry System
- **Test_Template**: A predefined form structure specific to each test type
- **Sample_ID**: Unique identifier for laboratory samples
- **Trial_Record**: Individual test attempt within a sample (up to 4 trials per test)
- **MTE_Equipment**: Measurement and Test Equipment used in laboratory testing
- **Historical_View**: Display of last 12 test results for reference
- **Common_Navigation**: Standard save/clear/delete operations available across all test types
- **Particle_Type**: Specific categories used in Inspect Filter and Ferrography tests

## Requirements

### Requirement 1

**User Story:** As a laboratory technician, I want to select and view sample information, so that I can enter test results for the correct sample.

#### Acceptance Criteria

1. WHEN a technician accesses the system, THE Lab_System SHALL display a list of available Sample_IDs for each test type
2. WHEN a Sample_ID is selected, THE Lab_System SHALL display the sample data template with relevant sample information
3. THE Lab_System SHALL provide access to common navigation menu buttons for all test operations
4. THE Lab_System SHALL display the appropriate Test_Template based on the selected test type

### Requirement 2

**User Story:** As a laboratory technician, I want to enter test results using trial-based data entry, so that I can record multiple attempts and ensure accuracy.

#### Acceptance Criteria

1. THE Lab_System SHALL provide four trial lines for results data entry with headers "Trial 1", "Trial 2", "Trial 3", "Trial 4"
2. WHEN entering numeric data, THE Lab_System SHALL validate that only numeric values are accepted
3. THE Lab_System SHALL allow selection of one or more trial records for Common_Navigation operations
4. THE Lab_System SHALL enable technicians to save, clear, or delete test result data entries
5. THE Lab_System SHALL update lab comments for each sample

### Requirement 3

**User Story:** As a laboratory technician, I want the system to automatically calculate derived values, so that I can ensure accurate results without manual computation.

#### Acceptance Criteria

1. WHEN Sample Weight and Final Buret values are entered for TAN tests, THE Lab_System SHALL calculate TAN using formula: (Final Buret * 5.61) / Sample Weight
2. WHEN Stop watch time and Tube ID are entered for Viscosity tests, THE Lab_System SHALL calculate cSt using formula: Stop watch time * Tube calibration value
3. WHEN Flash Point temperature and Barometric pressure are entered, THE Lab_System SHALL calculate Result using formula: Flash Point temperature + (0.06 * (760 - Barometric pressure))
4. THE Lab_System SHALL round calculated values to specified decimal places per test type
5. THE Lab_System SHALL set minimum values where specified (e.g., 0.01 for TAN if calculated result is less)

### Requirement 4

**User Story:** As a laboratory technician, I want to access measurement equipment dropdowns, so that I can properly record which equipment was used for testing.

#### Acceptance Criteria

1. THE Lab_System SHALL provide dropdown lists for MTE_Equipment selection where required by test templates
2. WHEN selecting thermometers, barometers, or other equipment, THE Lab_System SHALL populate dropdown from available MTE_Equipment records
3. THE Lab_System SHALL validate that required MTE_Equipment selections are made before saving results

### Requirement 5

**User Story:** As a laboratory technician, I want to upload and manage test data files, so that I can import results from laboratory instruments.

#### Acceptance Criteria

1. WHERE file upload is supported, THE Lab_System SHALL provide "Find File" functionality for each trial record
2. THE Lab_System SHALL allow preview of available files before upload
3. THE Lab_System SHALL associate uploaded files with specific Sample_ID and trial combinations
4. THE Lab_System SHALL support file formats used by laboratory instruments (.DAT, .TXT, etc.)

### Requirement 6

**User Story:** As a laboratory technician, I want to view historical test results, so that I can reference previous results and identify trends.

#### Acceptance Criteria

1. THE Lab_System SHALL display Historical_View showing last 12 Sample_IDs with sample date and Sample_ID
2. THE Lab_System SHALL provide header "Last 12 results for [Test Name]"
3. THE Lab_System SHALL allow resizing of the Historical_View section
4. THE Lab_System SHALL enable navigation to single screen mode for Historical_View
5. THE Lab_System SHALL provide access to older history beyond the last 12 results

### Requirement 7

**User Story:** As a laboratory technician, I want to work with specialized particle analysis tests, so that I can enter detailed particle characterization data.

#### Acceptance Criteria

1. WHERE Inspect Filter or Ferrography tests are selected, THE Lab_System SHALL display all predefined Particle_Types
2. THE Lab_System SHALL provide descriptions and image examples for each Particle_Type
3. THE Lab_System SHALL allow entry of Heat, Concentration, Size, Color, Texture, Composition, and Severity values for each Particle_Type
4. THE Lab_System SHALL calculate and display Overall Severity based on individual Particle_Type severities
5. THE Lab_System SHALL support filtered views (All Records vs Review) for Particle_Types
6. WHERE Ferrography is selected, THE Lab_System SHALL provide dilution factor options and partial save functionality

### Requirement 8

**User Story:** As a laboratory technician, I want to receive validation feedback, so that I can correct data entry errors before saving.

#### Acceptance Criteria

1. IF non-numeric values are entered in numeric fields, THEN THE Lab_System SHALL display appropriate error messages
2. IF required fields are left empty, THEN THE Lab_System SHALL prevent saving and highlight missing data
3. THE Lab_System SHALL display "NaN" or equivalent error indicators for invalid calculations
4. THE Lab_System SHALL validate that dropdown selections are made where required

### Requirement 9

**User Story:** As a laboratory technician, I want to perform lookup-based calculations, so that I can automatically determine standardized values.

#### Acceptance Criteria

1. WHEN particle count values are entered, THE Lab_System SHALL perform NAS lookup to determine the highest associated NAS value
2. WHEN grease penetration results are calculated, THE Lab_System SHALL perform NLGI lookup based on calculated result ranges
3. THE Lab_System SHALL maintain lookup tables for NAS and NLGI values
4. THE Lab_System SHALL display lookup results in read-only fields

### Requirement 10

**User Story:** As a system administrator, I want the system to integrate with the existing database schema, so that historical data and current operations remain accessible.

#### Acceptance Criteria

1. THE Lab_System SHALL connect to existing database tables, views, stored procedures, and functions
2. THE Lab_System SHALL handle tables without primary keys using raw SQL where necessary
3. THE Lab_System SHALL maintain compatibility with existing data structures and relationships
4. THE Lab_System SHALL support seeding of reference data from existing CSV files