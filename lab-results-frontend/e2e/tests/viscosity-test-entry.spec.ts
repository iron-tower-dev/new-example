import { test, expect } from '@playwright/test';

test.describe('Viscosity Test Entry Workflow', () => {
    test.beforeEach(async ({ page }) => {
        await page.goto('/');
        await page.waitForLoadState('networkidle');
    });

    test('should complete viscosity test entry workflow', async ({ page }) => {
        // Step 1: Navigate to Viscosity test entry
        await page.click('[data-testid="test-selection"]');
        await page.selectOption('[data-testid="test-dropdown"]', { label: 'Viscosity @ 40°C' });

        // Step 2: Select a sample
        await page.selectOption('[data-testid="sample-dropdown"]', { index: 1 });

        // Step 3: Select equipment
        await page.selectOption('[data-testid="thermometer-selection"]', { label: 'Digital Thermometer #1' });
        await page.selectOption('[data-testid="stopwatch-selection"]', { label: 'Digital Stopwatch #1' });
        await page.selectOption('[data-testid="tube-selection"]', { label: 'Viscometer Tube #1' });

        // Verify calibration values are displayed
        await expect(page.locator('[data-testid="tube-calibration"]')).toContainText('0.1234');

        // Step 4: Enter trial data
        // Trial 1
        await page.fill('[data-testid="trial-1-stopwatch-time"]', '120.5');

        // Verify calculation is performed automatically
        await expect(page.locator('[data-testid="trial-1-viscosity-result"]')).toHaveValue('14.87');

        // Trial 2
        await page.fill('[data-testid="trial-2-stopwatch-time"]', '118.2');
        await expect(page.locator('[data-testid="trial-2-viscosity-result"]')).toHaveValue('14.59');

        // Step 5: Add comments
        await page.fill('[data-testid="main-comments"]', 'Viscosity test completed at 40°C');

        // Step 6: Save the results
        await page.click('[data-testid="save-button"]');

        // Verify success message
        await expect(page.locator('[data-testid="success-message"]')).toBeVisible();
        await expect(page.locator('[data-testid="success-message"]')).toContainText('Results saved successfully');
    });

    test('should validate equipment selection', async ({ page }) => {
        // Navigate to Viscosity test entry
        await page.click('[data-testid="test-selection"]');
        await page.selectOption('[data-testid="test-dropdown"]', { label: 'Viscosity @ 40°C' });
        await page.selectOption('[data-testid="sample-dropdown"]', { index: 1 });

        // Try to save without selecting equipment
        await page.fill('[data-testid="trial-1-stopwatch-time"]', '120.5');
        await page.click('[data-testid="save-button"]');

        // Verify validation errors
        await expect(page.locator('[data-testid="validation-error"]')).toContainText('Thermometer selection is required');
        await expect(page.locator('[data-testid="validation-error"]')).toContainText('Tube selection is required');
    });

    test('should prevent selecting same thermometer twice', async ({ page }) => {
        // Navigate to Viscosity test entry
        await page.click('[data-testid="test-selection"]');
        await page.selectOption('[data-testid="test-dropdown"]', { label: 'Viscosity @ 40°C' });
        await page.selectOption('[data-testid="sample-dropdown"]', { index: 1 });

        // Select same thermometer for both fields
        await page.selectOption('[data-testid="thermometer-1-selection"]', { label: 'Digital Thermometer #1' });
        await page.selectOption('[data-testid="thermometer-2-selection"]', { label: 'Digital Thermometer #1' });

        // Verify validation error
        await expect(page.locator('[data-testid="validation-error"]')).toContainText('Must select different thermometers');
    });

    test('should show equipment calibration warnings', async ({ page }) => {
        // Navigate to Viscosity test entry
        await page.click('[data-testid="test-selection"]');
        await page.selectOption('[data-testid="test-dropdown"]', { label: 'Viscosity @ 40°C' });
        await page.selectOption('[data-testid="sample-dropdown"]', { index: 1 });

        // Select overdue equipment
        await page.selectOption('[data-testid="thermometer-selection"]', { label: 'Digital Thermometer #3 - OVERDUE' });

        // Verify warning is displayed
        await expect(page.locator('[data-testid="equipment-warning"]')).toBeVisible();
        await expect(page.locator('[data-testid="equipment-warning"]')).toContainText('Equipment is overdue for calibration');
    });

    test('should validate stopwatch time input', async ({ page }) => {
        // Navigate to Viscosity test entry
        await page.click('[data-testid="test-selection"]');
        await page.selectOption('[data-testid="test-dropdown"]', { label: 'Viscosity @ 40°C' });
        await page.selectOption('[data-testid="sample-dropdown"]', { index: 1 });

        // Select equipment
        await page.selectOption('[data-testid="thermometer-selection"]', { label: 'Digital Thermometer #1' });
        await page.selectOption('[data-testid="tube-selection"]', { label: 'Viscometer Tube #1' });

        // Enter invalid stopwatch time
        await page.fill('[data-testid="trial-1-stopwatch-time"]', '0');

        // Verify validation error
        await expect(page.locator('[data-testid="validation-error"]')).toContainText('Stopwatch time must be greater than zero');

        // Enter negative value
        await page.fill('[data-testid="trial-1-stopwatch-time"]', '-10');
        await expect(page.locator('[data-testid="validation-error"]')).toContainText('Stopwatch time must be greater than zero');
    });

    test('should show viscosity range warnings', async ({ page }) => {
        // Navigate to Viscosity test entry
        await page.click('[data-testid="test-selection"]');
        await page.selectOption('[data-testid="test-dropdown"]', { label: 'Viscosity @ 40°C' });
        await page.selectOption('[data-testid="sample-dropdown"]', { index: 1 });

        // Select equipment
        await page.selectOption('[data-testid="thermometer-selection"]', { label: 'Digital Thermometer #1' });
        await page.selectOption('[data-testid="tube-selection"]', { label: 'High Range Tube' }); // Assume this has high calibration value

        // Enter time that results in very high viscosity
        await page.fill('[data-testid="trial-1-stopwatch-time"]', '5000');

        // Verify warning for unusual viscosity value
        await expect(page.locator('[data-testid="validation-warning"]')).toContainText('Viscosity value seems unusually high');
    });

    test('should support repeatability validation for Q/QAG samples', async ({ page }) => {
        // Navigate to Viscosity test entry with Q/QAG sample
        await page.click('[data-testid="test-selection"]');
        await page.selectOption('[data-testid="test-dropdown"]', { label: 'Viscosity @ 40°C' });
        await page.selectOption('[data-testid="sample-dropdown"]', { label: 'TAG001 - Q Sample' });

        // Select equipment
        await page.selectOption('[data-testid="thermometer-selection"]', { label: 'Digital Thermometer #1' });
        await page.selectOption('[data-testid="tube-selection"]', { label: 'Viscometer Tube #1' });

        // Enter trial data with poor repeatability
        await page.fill('[data-testid="trial-1-stopwatch-time"]', '120.0');
        await page.fill('[data-testid="trial-2-stopwatch-time"]', '140.0'); // 20 second difference

        // Verify repeatability warning
        await expect(page.locator('[data-testid="validation-warning"]')).toContainText('Poor repeatability between trials');
    });

    test('should calculate average viscosity correctly', async ({ page }) => {
        // Navigate to Viscosity test entry
        await page.click('[data-testid="test-selection"]');
        await page.selectOption('[data-testid="test-dropdown"]', { label: 'Viscosity @ 40°C' });
        await page.selectOption('[data-testid="sample-dropdown"]', { index: 1 });

        // Select equipment
        await page.selectOption('[data-testid="thermometer-selection"]', { label: 'Digital Thermometer #1' });
        await page.selectOption('[data-testid="tube-selection"]', { label: 'Viscometer Tube #1' });

        // Enter multiple trials
        await page.fill('[data-testid="trial-1-stopwatch-time"]', '120.0');
        await page.fill('[data-testid="trial-2-stopwatch-time"]', '121.0');
        await page.fill('[data-testid="trial-3-stopwatch-time"]', '119.0');

        // Verify individual calculations
        await expect(page.locator('[data-testid="trial-1-viscosity-result"]')).toHaveValue('14.81');
        await expect(page.locator('[data-testid="trial-2-viscosity-result"]')).toHaveValue('14.93');
        await expect(page.locator('[data-testid="trial-3-viscosity-result"]')).toHaveValue('14.68');

        // Verify average calculation
        await expect(page.locator('[data-testid="average-viscosity"]')).toContainText('14.81');
    });

    test('should handle different viscosity tube calibrations', async ({ page }) => {
        // Navigate to Viscosity test entry
        await page.click('[data-testid="test-selection"]');
        await page.selectOption('[data-testid="test-dropdown"]', { label: 'Viscosity @ 40°C' });
        await page.selectOption('[data-testid="sample-dropdown"]', { index: 1 });

        // Test with different tube calibrations
        const tubes = [
            { name: 'Low Range Tube', calibration: '0.0123', expectedResult: '1.48' },
            { name: 'Medium Range Tube', calibration: '0.1234', expectedResult: '14.81' },
            { name: 'High Range Tube', calibration: '1.2345', expectedResult: '148.14' }
        ];

        for (const tube of tubes) {
            await page.selectOption('[data-testid="tube-selection"]', { label: tube.name });

            // Verify calibration value is displayed
            await expect(page.locator('[data-testid="tube-calibration"]')).toContainText(tube.calibration);

            // Enter stopwatch time
            await page.fill('[data-testid="trial-1-stopwatch-time"]', '120.0');

            // Verify calculated result
            await expect(page.locator('[data-testid="trial-1-viscosity-result"]')).toHaveValue(tube.expectedResult);

            // Clear for next iteration
            await page.fill('[data-testid="trial-1-stopwatch-time"]', '');
        }
    });
});