import { test, expect } from '@playwright/test';

test.describe('Water-KF Test Entry Workflow', () => {
    test.beforeEach(async ({ page }) => {
        await page.goto('/');
        await page.waitForLoadState('networkidle');
    });

    test('should complete Water-KF test entry workflow', async ({ page }) => {
        // Step 1: Navigate to Water-KF test entry
        await page.click('[data-testid="test-selection"]');
        await page.selectOption('[data-testid="test-dropdown"]', { label: 'Water-KF' });

        // Step 2: Select a sample
        await page.selectOption('[data-testid="sample-dropdown"]', { index: 1 });

        // Step 3: Enter trial data (4 trials for Water-KF)
        const trialValues = ['0.05', '0.048', '0.052', '0.049'];

        for (let i = 0; i < trialValues.length; i++) {
            await page.fill(`[data-testid="trial-${i + 1}-water-content"]`, trialValues[i]);
        }

        // Verify average calculation
        const expectedAverage = (0.05 + 0.048 + 0.052 + 0.049) / 4;
        await expect(page.locator('[data-testid="average-water-content"]')).toContainText(expectedAverage.toFixed(3));

        // Step 4: Upload Karl Fischer data file
        const fileInput = page.locator('[data-testid="trial-1-file-upload"]');
        await fileInput.setInputFiles('e2e/test-data/water-kf-data.dat');

        // Verify file upload success
        await expect(page.locator('[data-testid="file-upload-success"]')).toBeVisible();

        // Step 5: Add comments
        await page.fill('[data-testid="main-comments"]', 'Karl Fischer water analysis completed');

        // Step 6: Save the results
        await page.click('[data-testid="save-button"]');

        // Verify success message
        await expect(page.locator('[data-testid="success-message"]')).toBeVisible();
        await expect(page.locator('[data-testid="success-message"]')).toContainText('Results saved successfully');
    });

    test('should validate water content ranges', async ({ page }) => {
        // Navigate to Water-KF test entry
        await page.click('[data-testid="test-selection"]');
        await page.selectOption('[data-testid="test-dropdown"]', { label: 'Water-KF' });
        await page.selectOption('[data-testid="sample-dropdown"]', { index: 1 });

        // Enter negative water content
        await page.fill('[data-testid="trial-1-water-content"]', '-0.01');

        // Verify validation error
        await expect(page.locator('[data-testid="validation-error"]')).toContainText('Water content cannot be negative');

        // Enter extremely high water content
        await page.fill('[data-testid="trial-1-water-content"]', '10.0');

        // Verify warning for high water content
        await expect(page.locator('[data-testid="validation-warning"]')).toContainText('Water content seems unusually high');
        await expect(page.locator('[data-testid="validation-warning"]')).toContainText('Consider sample contamination');
    });

    test('should validate numeric input format', async ({ page }) => {
        // Navigate to Water-KF test entry
        await page.click('[data-testid="test-selection"]');
        await page.selectOption('[data-testid="test-dropdown"]', { label: 'Water-KF' });
        await page.selectOption('[data-testid="sample-dropdown"]', { index: 1 });

        // Enter non-numeric values
        await page.fill('[data-testid="trial-1-water-content"]', 'abc');
        await page.fill('[data-testid="trial-2-water-content"]', 'xyz');

        // Verify validation errors
        await expect(page.locator('[data-testid="validation-error"]')).toContainText('Must be a valid number');

        // Enter values with too many decimal places
        await page.fill('[data-testid="trial-1-water-content"]', '0.123456789');

        // Verify precision warning
        await expect(page.locator('[data-testid="validation-warning"]')).toContainText('Value will be rounded to 3 decimal places');
    });

    test('should handle file upload for Karl Fischer data', async ({ page }) => {
        // Navigate to Water-KF test entry
        await page.click('[data-testid="test-selection"]');
        await page.selectOption('[data-testid="test-dropdown"]', { label: 'Water-KF' });
        await page.selectOption('[data-testid="sample-dropdown"]', { index: 1 });

        // Test file upload for each trial
        for (let trial = 1; trial <= 4; trial++) {
            const fileInput = page.locator(`[data-testid="trial-${trial}-file-upload"]`);
            await fileInput.setInputFiles('e2e/test-data/water-kf-data.dat');

            // Verify file preview
            await expect(page.locator(`[data-testid="trial-${trial}-file-preview"]`)).toBeVisible();
            await expect(page.locator(`[data-testid="trial-${trial}-file-name"]`)).toContainText('water-kf-data.dat');

            // Test file removal
            await page.click(`[data-testid="trial-${trial}-remove-file"]`);
            await expect(page.locator(`[data-testid="trial-${trial}-file-preview"]`)).not.toBeVisible();
        }
    });

    test('should validate file format for Karl Fischer data', async ({ page }) => {
        // Navigate to Water-KF test entry
        await page.click('[data-testid="test-selection"]');
        await page.selectOption('[data-testid="test-dropdown"]', { label: 'Water-KF' });
        await page.selectOption('[data-testid="sample-dropdown"]', { index: 1 });

        // Try to upload invalid file format
        const fileInput = page.locator('[data-testid="trial-1-file-upload"]');

        // Create a temporary invalid file for testing
        await page.evaluate(() => {
            const file = new File(['invalid content'], 'invalid.txt', { type: 'text/plain' });
            const input = document.querySelector('[data-testid="trial-1-file-upload"]') as HTMLInputElement;
            const dataTransfer = new DataTransfer();
            dataTransfer.items.add(file);
            input.files = dataTransfer.files;
            input.dispatchEvent(new Event('change', { bubbles: true }));
        });

        // Verify file format validation
        await expect(page.locator('[data-testid="file-validation-error"]')).toContainText('Only .DAT files are supported for Karl Fischer data');
    });

    test('should calculate repeatability statistics', async ({ page }) => {
        // Navigate to Water-KF test entry
        await page.click('[data-testid="test-selection"]');
        await page.selectOption('[data-testid="test-dropdown"]', { label: 'Water-KF' });
        await page.selectOption('[data-testid="sample-dropdown"]', { index: 1 });

        // Enter trial data with good repeatability
        await page.fill('[data-testid="trial-1-water-content"]', '0.050');
        await page.fill('[data-testid="trial-2-water-content"]', '0.051');
        await page.fill('[data-testid="trial-3-water-content"]', '0.049');
        await page.fill('[data-testid="trial-4-water-content"]', '0.052');

        // Verify statistics calculations
        await expect(page.locator('[data-testid="average-water-content"]')).toContainText('0.051');
        await expect(page.locator('[data-testid="standard-deviation"]')).toContainText('0.001');
        await expect(page.locator('[data-testid="relative-standard-deviation"]')).toContainText('2.4%');

        // Verify good repeatability indicator
        await expect(page.locator('[data-testid="repeatability-status"]')).toContainText('Good repeatability');
    });

    test('should warn about poor repeatability', async ({ page }) => {
        // Navigate to Water-KF test entry
        await page.click('[data-testid="test-selection"]');
        await page.selectOption('[data-testid="test-dropdown"]', { label: 'Water-KF' });
        await page.selectOption('[data-testid="sample-dropdown"]', { index: 1 });

        // Enter trial data with poor repeatability
        await page.fill('[data-testid="trial-1-water-content"]', '0.020');
        await page.fill('[data-testid="trial-2-water-content"]', '0.080');
        await page.fill('[data-testid="trial-3-water-content"]', '0.040');
        await page.fill('[data-testid="trial-4-water-content"]', '0.100');

        // Verify poor repeatability warning
        await expect(page.locator('[data-testid="validation-warning"]')).toContainText('Poor repeatability between trials');
        await expect(page.locator('[data-testid="validation-warning"]')).toContainText('Consider repeating the analysis');
        await expect(page.locator('[data-testid="repeatability-status"]')).toContainText('Poor repeatability');
    });

    test('should support partial data entry and auto-save', async ({ page }) => {
        // Navigate to Water-KF test entry
        await page.click('[data-testid="test-selection"]');
        await page.selectOption('[data-testid="test-dropdown"]', { label: 'Water-KF' });
        await page.selectOption('[data-testid="sample-dropdown"]', { index: 1 });

        // Enter partial data
        await page.fill('[data-testid="trial-1-water-content"]', '0.050');
        await page.fill('[data-testid="trial-2-water-content"]', '0.051');

        // Verify partial save functionality
        await page.click('[data-testid="partial-save-button"]');
        await expect(page.locator('[data-testid="partial-save-success"]')).toContainText('Progress saved');

        // Reload page and verify data persistence
        await page.reload();
        await expect(page.locator('[data-testid="trial-1-water-content"]')).toHaveValue('0.050');
        await expect(page.locator('[data-testid="trial-2-water-content"]')).toHaveValue('0.051');
        await expect(page.locator('[data-testid="trial-3-water-content"]')).toHaveValue('');
        await expect(page.locator('[data-testid="trial-4-water-content"]')).toHaveValue('');
    });

    test('should show water content trend analysis', async ({ page }) => {
        // Navigate to Water-KF test entry
        await page.click('[data-testid="test-selection"]');
        await page.selectOption('[data-testid="test-dropdown"]', { label: 'Water-KF' });
        await page.selectOption('[data-testid="sample-dropdown"]', { index: 1 });

        // Enter water content value
        await page.fill('[data-testid="trial-1-water-content"]', '0.15');

        // Verify trend analysis is shown in historical results
        await expect(page.locator('[data-testid="trend-analysis"]')).toBeVisible();
        await expect(page.locator('[data-testid="trend-status"]')).toContainText('Increasing trend');
        await expect(page.locator('[data-testid="trend-recommendation"]')).toContainText('Monitor for water ingress');
    });
});