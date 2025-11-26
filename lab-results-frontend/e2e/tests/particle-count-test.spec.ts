import { test, expect } from '@playwright/test';

test.describe('Particle Count Test Entry Workflow', () => {
    test.beforeEach(async ({ page }) => {
        await page.goto('/');
        await page.waitForLoadState('networkidle');
    });

    test('should complete particle count test with NAS lookup', async ({ page }) => {
        // Navigate to Particle Count test entry
        await page.click('[data-testid="test-selection"]');
        await page.selectOption('[data-testid="test-dropdown"]', { label: 'Particle Count' });
        await page.selectOption('[data-testid="sample-dropdown"]', { index: 1 });

        // Enter particle count data
        await page.fill('[data-testid="particle-4"]', '1000');
        await page.fill('[data-testid="particle-6"]', '500');
        await page.fill('[data-testid="particle-14"]', '100');
        await page.fill('[data-testid="particle-21"]', '50');
        await page.fill('[data-testid="particle-38"]', '10');
        await page.fill('[data-testid="particle-70"]', '2');

        // Verify NAS calculation is performed automatically
        await expect(page.locator('[data-testid="nas-result"]')).toHaveValue('9');

        // Save results
        await page.click('[data-testid="save-button"]');
        await expect(page.locator('[data-testid="success-message"]')).toBeVisible();
    });

    test('should validate particle count inputs', async ({ page }) => {
        await page.click('[data-testid="test-selection"]');
        await page.selectOption('[data-testid="test-dropdown"]', { label: 'Particle Count' });
        await page.selectOption('[data-testid="sample-dropdown"]', { index: 1 });

        // Enter negative values
        await page.fill('[data-testid="particle-4"]', '-10');
        await expect(page.locator('[data-testid="validation-error"]')).toContainText('Particle count cannot be negative');

        // Enter very high values
        await page.fill('[data-testid="particle-4"]', '2000000');
        await expect(page.locator('[data-testid="validation-warning"]')).toContainText('Particle count seems unusually high');
    });

    test('should support file upload for particle counter data', async ({ page }) => {
        await page.click('[data-testid="test-selection"]');
        await page.selectOption('[data-testid="test-dropdown"]', { label: 'Particle Count' });
        await page.selectOption('[data-testid="sample-dropdown"]', { index: 1 });

        // Upload particle counter data file
        const fileInput = page.locator('[data-testid="file-upload"]');
        await fileInput.setInputFiles('test-data/particle-count-data.txt');

        // Verify data is parsed and populated
        await expect(page.locator('[data-testid="particle-4"]')).toHaveValue('1250');
        await expect(page.locator('[data-testid="nas-result"]')).toHaveValue('9');
    });
});