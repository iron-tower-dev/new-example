import { test, expect } from '@playwright/test';

test.describe('Emission Spectroscopy Test Entry Workflow', () => {
    test.beforeEach(async ({ page }) => {
        await page.goto('/');
        await page.waitForLoadState('networkidle');
    });

    test('should complete emission spectroscopy test entry workflow', async ({ page }) => {
        // Step 1: Navigate to Emission Spectroscopy test entry
        await page.click('[data-testid="test-selection"]');
        await page.selectOption('[data-testid="test-dropdown"]', { label: 'Emission Spectroscopy' });

        // Step 2: Select a sample
        await page.selectOption('[data-testid="sample-dropdown"]', { index: 1 });

        // Step 3: Enter element values for Trial 1
        const elements = ['Na', 'Cr', 'Sn', 'Si', 'Mo', 'Ca', 'Al', 'Ba', 'Mg', 'Ni', 'Mn', 'Zn', 'P', 'Ag', 'Pb', 'H', 'B', 'Cu', 'Fe'];

        for (let i = 0; i < elements.length; i++) {
            const element = elements[i];
            const value = (i + 1) * 10; // Different values for each element
            await page.fill(`[data-testid="trial-1-${element.toLowerCase()}"]`, value.toString());
        }

        // Step 4: Check Ferrography scheduling option
        await page.check('[data-testid="trial-1-schedule-ferrography"]');

        // Step 5: Upload spectroscopy data file
        const fileInput = page.locator('[data-testid="trial-1-file-upload"]');
        await fileInput.setInputFiles('e2e/test-data/spectroscopy-data.txt');

        // Verify file upload success
        await expect(page.locator('[data-testid="file-upload-success"]')).toBeVisible();

        // Step 6: Add comments
        await page.fill('[data-testid="main-comments"]', 'Emission spectroscopy analysis completed');

        // Step 7: Save the results
        await page.click('[data-testid="save-button"]');

        // Verify success message
        await expect(page.locator('[data-testid="success-message"]')).toBeVisible();
        await expect(page.locator('[data-testid="success-message"]')).toContainText('Results saved successfully');

        // Verify Ferrography was scheduled
        await expect(page.locator('[data-testid="ferrography-scheduled"]')).toContainText('Ferrography test has been scheduled');
    });

    test('should validate element value ranges', async ({ page }) => {
        // Navigate to Emission Spectroscopy test entry
        await page.click('[data-testid="test-selection"]');
        await page.selectOption('[data-testid="test-dropdown"]', { label: 'Emission Spectroscopy' });
        await page.selectOption('[data-testid="sample-dropdown"]', { index: 1 });

        // Enter negative values
        await page.fill('[data-testid="trial-1-fe"]', '-10');
        await page.fill('[data-testid="trial-1-cu"]', '-5');

        // Verify validation errors
        await expect(page.locator('[data-testid="validation-error"]')).toContainText('Element values cannot be negative');

        // Enter extremely high values
        await page.fill('[data-testid="trial-1-fe"]', '10000');
        await expect(page.locator('[data-testid="validation-warning"]')).toContainText('Iron value seems unusually high');
    });

    test('should support multiple trials with different values', async ({ page }) => {
        // Navigate to Emission Spectroscopy test entry
        await page.click('[data-testid="test-selection"]');
        await page.selectOption('[data-testid="test-dropdown"]', { label: 'Emission Spectroscopy' });
        await page.selectOption('[data-testid="sample-dropdown"]', { index: 1 });

        // Enter values for multiple trials
        await page.fill('[data-testid="trial-1-fe"]', '25');
        await page.fill('[data-testid="trial-1-cu"]', '15');
        await page.fill('[data-testid="trial-1-al"]', '8');

        await page.fill('[data-testid="trial-2-fe"]', '27');
        await page.fill('[data-testid="trial-2-cu"]', '16');
        await page.fill('[data-testid="trial-2-al"]', '9');

        // Verify average calculations are displayed
        await expect(page.locator('[data-testid="average-fe"]')).toContainText('26');
        await expect(page.locator('[data-testid="average-cu"]')).toContainText('15.5');
        await expect(page.locator('[data-testid="average-al"]')).toContainText('8.5');
    });

    test('should handle file upload for spectroscopy data', async ({ page }) => {
        // Navigate to Emission Spectroscopy test entry
        await page.click('[data-testid="test-selection"]');
        await page.selectOption('[data-testid="test-dropdown"]', { label: 'Emission Spectroscopy' });
        await page.selectOption('[data-testid="sample-dropdown"]', { index: 1 });

        // Test file upload
        const fileInput = page.locator('[data-testid="trial-1-file-upload"]');
        await fileInput.setInputFiles('e2e/test-data/spectroscopy-data.txt');

        // Verify file preview
        await expect(page.locator('[data-testid="file-preview"]')).toBeVisible();
        await expect(page.locator('[data-testid="file-name"]')).toContainText('spectroscopy-data.txt');

        // Test file removal
        await page.click('[data-testid="remove-file"]');
        await expect(page.locator('[data-testid="file-preview"]')).not.toBeVisible();
    });

    test('should only allow Ferrography scheduling in Trial 1', async ({ page }) => {
        // Navigate to Emission Spectroscopy test entry
        await page.click('[data-testid="test-selection"]');
        await page.selectOption('[data-testid="test-dropdown"]', { label: 'Emission Spectroscopy' });
        await page.selectOption('[data-testid="sample-dropdown"]', { index: 1 });

        // Verify Ferrography checkbox is only available in Trial 1
        await expect(page.locator('[data-testid="trial-1-schedule-ferrography"]')).toBeVisible();
        await expect(page.locator('[data-testid="trial-2-schedule-ferrography"]')).not.toBeVisible();
        await expect(page.locator('[data-testid="trial-3-schedule-ferrography"]')).not.toBeVisible();
        await expect(page.locator('[data-testid="trial-4-schedule-ferrography"]')).not.toBeVisible();
    });

    test('should validate required elements for specific sample types', async ({ page }) => {
        // Navigate to Emission Spectroscopy test entry
        await page.click('[data-testid="test-selection"]');
        await page.selectOption('[data-testid="test-dropdown"]', { label: 'Emission Spectroscopy' });
        await page.selectOption('[data-testid="sample-dropdown"]', { label: 'Engine Oil Sample' });

        // Try to save without entering critical elements
        await page.click('[data-testid="save-button"]');

        // Verify validation for critical elements
        await expect(page.locator('[data-testid="validation-error"]')).toContainText('Iron (Fe) is required for engine oil samples');
        await expect(page.locator('[data-testid="validation-error"]')).toContainText('Copper (Cu) is required for engine oil samples');
    });

    test('should show element trend warnings', async ({ page }) => {
        // Navigate to Emission Spectroscopy test entry
        await page.click('[data-testid="test-selection"]');
        await page.selectOption('[data-testid="test-dropdown"]', { label: 'Emission Spectroscopy' });
        await page.selectOption('[data-testid="sample-dropdown"]', { index: 1 });

        // Enter high wear metal values
        await page.fill('[data-testid="trial-1-fe"]', '150'); // High iron
        await page.fill('[data-testid="trial-1-cu"]', '80');  // High copper
        await page.fill('[data-testid="trial-1-pb"]', '60');  // High lead

        // Verify trend warnings
        await expect(page.locator('[data-testid="trend-warning"]')).toContainText('High wear metal concentrations detected');
        await expect(page.locator('[data-testid="trend-warning"]')).toContainText('Consider scheduling additional analysis');
    });
});