# End-to-End Testing

This directory contains Playwright end-to-end tests for the Laboratory Test Results Entry System.

## Test Structure

- `tests/` - Contains all E2E test files
- `test-data/` - Contains sample data files used in tests
- `playwright.config.ts` - Playwright configuration

## Test Coverage

### Core Workflows
- **TAN Test Entry** (`tan-test-entry.spec.ts`)
  - Complete test entry workflow
  - Field validation
  - Calculation verification
  - Data persistence

- **Viscosity Test Entry** (`viscosity-test-entry.spec.ts`)
  - Equipment selection and validation
  - Calibration value handling
  - Repeatability checks
  - Multiple trial calculations

- **Particle Count Test** (`particle-count-test.spec.ts`)
  - Particle count data entry
  - NAS lookup calculations
  - File upload integration

### Supporting Features
- **File Upload Workflow** (`file-upload-workflow.spec.ts`)
  - File upload and preview
  - File type and size validation
  - Drag and drop functionality

- **Historical Results** (`historical-results.spec.ts`)
  - Historical data display
  - Resizable panels
  - Single screen mode
  - Extended history access

- **Cross-Browser Compatibility** (`cross-browser-compatibility.spec.ts`)
  - Chrome, Firefox, Safari testing
  - Responsive design validation
  - Keyboard navigation

## Running Tests

```bash
# Run all E2E tests
npm run e2e

# Run tests with browser UI
npm run e2e:headed

# Run tests with Playwright UI
npm run e2e:ui

# Run specific test file
npx playwright test tan-test-entry.spec.ts

# Run tests in specific browser
npx playwright test --project=chromium
```

## Test Data

The `test-data/` directory contains sample files used in tests:
- `water-kf-data.dat` - Karl Fischer water content data
- `particle-count-data.txt` - Particle counter results
- Additional test files as needed

## Prerequisites

1. Angular development server running on `http://localhost:4200`
2. Backend API server running on `http://localhost:5000`
3. Test database with sample data

## Test Patterns

### Page Object Model
Tests use data-testid attributes for reliable element selection:
```typescript
await page.click('[data-testid="save-button"]');
await expect(page.locator('[data-testid="success-message"]')).toBeVisible();
```

### Workflow Testing
Each test follows a complete user workflow:
1. Navigation to test type
2. Sample selection
3. Data entry
4. Validation
5. Save and verification

### Cross-Browser Testing
Tests run across multiple browsers to ensure compatibility:
- Chromium (Chrome)
- Firefox
- WebKit (Safari)
- Mobile browsers

## Maintenance

- Update test data files when data formats change
- Add new test files for new features
- Update selectors if UI changes
- Review and update browser compatibility matrix