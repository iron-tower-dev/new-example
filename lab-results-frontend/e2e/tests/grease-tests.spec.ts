import { test, expect } from '@playwright/test';

test.describe('Grease Tests Workflow', () => {
    test.beforeEach(async ({ page }) => {
        await page.goto('/');
        await page.waitForLoadState('networkidle');
    });

    test('should complete grease penetration test entry workflow', async ({ page }) => {
        // Step 1: Navigate to Grease Penetration test entry
        await page.click('[data-testid="test-selection"]');
        await page.selectOption('[data-testid="test-dropdown"]', { label: 'Grease Penetration Worked' });

        // Step 2: Select a grease sample
        await page.selectOption('[data-testid="sample-dropdown"]', { label: 'GREASE001 - Lithium Grease' });

        // Step 3: Enter penetration values
        await page.fill('[data-testid="trial-1-penetration-1"]', '250');
        await page.fill('[data-testid="trial-1-penetration-2"]', '255');
        await page.fill('[data-testid="trial-1-penetration-3"]', '252');

        // Verify calculation is performed automatically
        // Expected: ((250 + 255 + 252) / 3) * 3.75 + 24 = 946.125 + 24 = 970.125, rounded to 970
        await expect(page.locator('[data-testid="trial-1-penetration-result"]')).toHaveValue('970');

        // Verify NLGI lookup is performed
        await expect(page.locator('[data-testid="trial-1-nlgi-grade"]')).toContainText('2');

        // Step 4: Add comments
        await page.fill('[data-testid="main-comments"]', 'Grease penetration test completed');

        // Step 5: Save the results
        await page.click('[data-testid="save-button"]');

        // Verify success message
        await expect(page.locator('[data-testid="success-message"]')).toBeVisible();
    });

    test('should complete grease dropping point test entry workflow', async ({ page }) => {
        // Step 1: Navigate to Grease Dropping Point test entry
        await page.click('[data-testid="test-selection"]');
        await page.selectOption('[data-testid="test-dropdown"]', { label: 'Grease Dropping Point' });

        // Step 2: Select a grease sample
        await page.selectOption('[data-testid="sample-dropdown"]', { label: 'GREASE001 - Lithium Grease' });

        // Step 3: Select thermometers (must be different)
        await page.selectOption('[data-testid="thermometer-1-selection"]', { label: 'Digital Thermometer #1' });
        await page.selectOption('[data-testid="thermometer-2-selection"]', { label: 'Digital Thermometer #2' });

        // Step 4: Enter temperature values
        await page.fill('[data-testid="trial-1-dropping-point"]', '180');
        await page.fill('[data-testid="trial-1-block-temp"]', '185');

        // Verify calculation is performed automatically
        // Expected: 180 + ((185 - 180) / 3) = 180 + 1.67 = 181.67
        await expect(page.locator('[data-testid="trial-1-corrected-dropping-point"]')).toHaveValue('181.67');

        // Step 5: Save the results
        await page.click('[data-testid="save-button"]');

        // Verify success message
        await expect(page.locator('[data-testid="success-message"]')).toBeVisible();
    });

    test('should validate grease penetration input ranges', async ({ page }) => {
        // Navigate to Grease Penetration test entry
        await page.click('[data-testid="test-selection"]');
        await page.selectOption('[data-testid="test-dropdown"]', { label: 'Grease Penetration Worked' });
        await page.selectOption('[data-testid="sample-dropdown"]', { index: 1 });

        // Enter invalid penetration values
        await page.fill('[data-testid="trial-1-penetration-1"]', '0');
        await page.fill('[data-testid="trial-1-penetration-2"]', '-10');
        await page.fill('[data-testid="trial-1-penetration-3"]', '1000');

        // Verify validation errors
        await expect(page.locator('[data-testid="validation-error"]')).toContainText('Penetration values must be greater than zero');
        await expect(page.locator('[data-testid="validation-error"]')).toContainText('Penetration values cannot be negative');
        await expect(page.locator('[data-testid="validation-warning"]')).toContainText('Penetration value seems unusually high');
    });

    test('should prevent selecting same thermometer for dropping point test', async ({ page }) => {
        // Navigate to Grease Dropping Point test entry
        await page.click('[data-testid="test-selection"]');
        await page.selectOption('[data-testid="test-dropdown"]', { label: 'Grease Dropping Point' });
        await page.selectOption('[data-testid="sample-dropdown"]', { index: 1 });

        // Try to select same thermometer for both fields
        await page.selectOption('[data-testid="thermometer-1-selection"]', { label: 'Digital Thermometer #1' });
        await page.selectOption('[data-testid="thermometer-2-selection"]', { label: 'Digital Thermometer #1' });

        // Verify validation error
        await expect(page.locator('[data-testid="validation-error"]')).toContainText('Must select different thermometers');
    });

    test('should validate dropping point temperature ranges', async ({ page }) => {
        // Navigate to Grease Dropping Point test entry
        await page.click('[data-testid="test-selection"]');
        await page.selectOption('[data-testid="test-dropdown"]', { label: 'Grease Dropping Point' });
        await page.selectOption('[data-testid="sample-dropdown"]', { index: 1 });

        // Select thermometers
        await page.selectOption('[data-testid="thermometer-1-selection"]', { label: 'Digital Thermometer #1' });
        await page.selectOption('[data-testid="thermometer-2-selection"]', { label: 'Digital Thermometer #2' });

        // Enter invalid temperature values
        await page.fill('[data-testid="trial-1-dropping-point"]', '0');
        await page.fill('[data-testid="trial-1-block-temp"]', '-10');

        // Verify validation errors
        await expect(page.locator('[data-testid="validation-error"]')).toContainText('Dropping point must be greater than zero');
        await expect(page.locator('[data-testid="validation-error"]')).toContainText('Block temperature cannot be negative');

        // Enter block temp lower than dropping point
        await page.fill('[data-testid="trial-1-dropping-point"]', '200');
        await page.fill('[data-testid="trial-1-block-temp"]', '190');

        // Verify logical validation
        await expect(page.locator('[data-testid="validation-warning"]')).toContainText('Block temperature is typically higher than dropping point');
    });

    test('should show NLGI grade lookup results', async ({ page }) => {
        // Navigate to Grease Penetration test entry
        await page.click('[data-testid="test-selection"]');
        await page.selectOption('[data-testid="test-dropdown"]', { label: 'Grease Penetration Worked' });
        await page.selectOption('[data-testid="sample-dropdown"]', { index: 1 });

        // Test different penetration ranges for different NLGI grades
        const testCases = [
            { penetrations: ['370', '375', '380'], expectedGrade: '00' },
            { penetrations: ['320', '325', '330'], expectedGrade: '0' },
            { penetrations: ['270', '275', '280'], expectedGrade: '1' },
            { penetrations: ['220', '225', '230'], expectedGrade: '2' }
        ];

        for (const testCase of testCases) {
            // Clear previous values
            await page.fill('[data-testid="trial-1-penetration-1"]', '');
            await page.fill('[data-testid="trial-1-penetration-2"]', '');
            await page.fill('[data-testid="trial-1-penetration-3"]', '');

            // Enter new values
            await page.fill('[data-testid="trial-1-penetration-1"]', testCase.penetrations[0]);
            await page.fill('[data-testid="trial-1-penetration-2"]', testCase.penetrations[1]);
            await page.fill('[data-testid="trial-1-penetration-3"]', testCase.penetrations[2]);

            // Verify NLGI grade lookup
            await expect(page.locator('[data-testid="trial-1-nlgi-grade"]')).toContainText(testCase.expectedGrade);
        }
    });

    test('should handle penetration repeatability validation', async ({ page }) => {
        // Navigate to Grease Penetration test entry
        await page.click('[data-testid="test-selection"]');
        await page.selectOption('[data-testid="test-dropdown"]', { label: 'Grease Penetration Worked' });
        await page.selectOption('[data-testid="sample-dropdown"]', { index: 1 });

        // Enter values with poor repeatability
        await page.fill('[data-testid="trial-1-penetration-1"]', '200');
        await page.fill('[data-testid="trial-1-penetration-2"]', '250');
        await page.fill('[data-testid="trial-1-penetration-3"]', '300');

        // Verify repeatability warning
        await expect(page.locator('[data-testid="validation-warning"]')).toContainText('Large variation between penetration readings');
        await expect(page.locator('[data-testid="validation-warning"]')).toContainText('Consider repeating the test');
    });

    test('should show equipment calibration warnings for thermometers', async ({ page }) => {
        // Navigate to Grease Dropping Point test entry
        await page.click('[data-testid="test-selection"]');
        await page.selectOption('[data-testid="test-dropdown"]', { label: 'Grease Dropping Point' });
        await page.selectOption('[data-testid="sample-dropdown"]', { index: 1 });

        // Select overdue thermometer
        await page.selectOption('[data-testid="thermometer-1-selection"]', { label: 'Digital Thermometer #3 - OVERDUE' });

        // Verify warning is displayed
        await expect(page.locator('[data-testid="equipment-warning"]')).toBeVisible();
        await expect(page.locator('[data-testid="equipment-warning"]')).toContainText('Thermometer is overdue for calibration');
    });

    test('should calculate multiple trials correctly for grease tests', async ({ page }) => {
        // Navigate to Grease Penetration test entry
        await page.click('[data-testid="test-selection"]');
        await page.selectOption('[data-testid="test-dropdown"]', { label: 'Grease Penetration Worked' });
        await page.selectOption('[data-testid="sample-dropdown"]', { index: 1 });

        // Enter values for multiple trials
        // Trial 1
        await page.fill('[data-testid="trial-1-penetration-1"]', '250');
        await page.fill('[data-testid="trial-1-penetration-2"]', '255');
        await page.fill('[data-testid="trial-1-penetration-3"]', '252');

        // Trial 2
        await page.fill('[data-testid="trial-2-penetration-1"]', '248');
        await page.fill('[data-testid="trial-2-penetration-2"]', '253');
        await page.fill('[data-testid="trial-2-penetration-3"]', '250');

        // Verify individual trial calculations
        await expect(page.locator('[data-testid="trial-1-penetration-result"]')).toHaveValue('970');
        await expect(page.locator('[data-testid="trial-2-penetration-result"]')).toHaveValue('966');

        // Verify average calculation
        await expect(page.locator('[data-testid="average-penetration"]')).toContainText('968');
    });
});