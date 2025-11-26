import { test, expect } from '@playwright/test';

test.describe('TAN Test Entry Workflow', () => {
    test.beforeEach(async ({ page }) => {
        // Navigate to the application
        await page.goto('/');

        // Assume we need to login first
        await page.waitForLoadState('networkidle');
    });

    test('should complete TAN test entry workflow', async ({ page }) => {
        // Step 1: Navigate to TAN test entry
        await page.click('[data-testid="test-selection"]');
        await page.selectOption('[data-testid="test-dropdown"]', { label: 'TAN by Color Indication' });

        // Step 2: Select a sample
        await page.click('[data-testid="sample-selection"]');
        await page.selectOption('[data-testid="sample-dropdown"]', { index: 1 });

        // Verify sample information is displayed
        await expect(page.locator('[data-testid="sample-info"]')).toBeVisible();
        await expect(page.locator('[data-testid="tag-number"]')).toContainText('TAG');

        // Step 3: Enter trial data
        // Trial 1
        await page.fill('[data-testid="trial-1-sample-weight"]', '10.0');
        await page.fill('[data-testid="trial-1-final-buret"]', '5.5');

        // Verify calculation is performed
        await expect(page.locator('[data-testid="trial-1-tan-result"]')).toHaveValue('3.08');

        // Trial 2
        await page.fill('[data-testid="trial-2-sample-weight"]', '10.5');
        await page.fill('[data-testid="trial-2-final-buret"]', '5.8');

        // Verify calculation is performed
        await expect(page.locator('[data-testid="trial-2-tan-result"]')).toHaveValue('3.10');

        // Step 4: Add comments
        await page.fill('[data-testid="main-comments"]', 'Test completed successfully');

        // Step 5: Save the results
        await page.click('[data-testid="save-button"]');

        // Verify success message
        await expect(page.locator('[data-testid="success-message"]')).toBeVisible();
        await expect(page.locator('[data-testid="success-message"]')).toContainText('Results saved successfully');

        // Step 6: Verify data persistence
        await page.reload();
        await expect(page.locator('[data-testid="trial-1-sample-weight"]')).toHaveValue('10.0');
        await expect(page.locator('[data-testid="trial-1-final-buret"]')).toHaveValue('5.5');
        await expect(page.locator('[data-testid="trial-1-tan-result"]')).toHaveValue('3.08');
    });

    test('should validate required fields', async ({ page }) => {
        // Navigate to TAN test entry
        await page.click('[data-testid="test-selection"]');
        await page.selectOption('[data-testid="test-dropdown"]', { label: 'TAN by Color Indication' });
        await page.selectOption('[data-testid="sample-dropdown"]', { index: 1 });

        // Try to save without entering data
        await page.click('[data-testid="save-button"]');

        // Verify validation errors
        await expect(page.locator('[data-testid="validation-error"]')).toBeVisible();
        await expect(page.locator('[data-testid="validation-error"]')).toContainText('Sample weight is required');

        // Enter only sample weight
        await page.fill('[data-testid="trial-1-sample-weight"]', '10.0');
        await page.click('[data-testid="save-button"]');

        // Verify final buret validation
        await expect(page.locator('[data-testid="validation-error"]')).toContainText('Final buret reading is required');
    });

    test('should validate numeric inputs', async ({ page }) => {
        // Navigate to TAN test entry
        await page.click('[data-testid="test-selection"]');
        await page.selectOption('[data-testid="test-dropdown"]', { label: 'TAN by Color Indication' });
        await page.selectOption('[data-testid="sample-dropdown"]', { index: 1 });

        // Enter non-numeric values
        await page.fill('[data-testid="trial-1-sample-weight"]', 'abc');
        await page.fill('[data-testid="trial-1-final-buret"]', 'xyz');

        // Verify validation errors
        await expect(page.locator('[data-testid="validation-error"]')).toContainText('Must be a valid number');

        // Enter zero sample weight
        await page.fill('[data-testid="trial-1-sample-weight"]', '0');
        await page.fill('[data-testid="trial-1-final-buret"]', '5.5');

        // Verify zero validation
        await expect(page.locator('[data-testid="validation-error"]')).toContainText('Sample weight must be greater than zero');
    });

    test('should show calculation warnings for unusual values', async ({ page }) => {
        // Navigate to TAN test entry
        await page.click('[data-testid="test-selection"]');
        await page.selectOption('[data-testid="test-dropdown"]', { label: 'TAN by Color Indication' });
        await page.selectOption('[data-testid="sample-dropdown"]', { index: 1 });

        // Enter values that would result in very high TAN
        await page.fill('[data-testid="trial-1-sample-weight"]', '1.0');
        await page.fill('[data-testid="trial-1-final-buret"]', '20.0');

        // Verify warning is shown
        await expect(page.locator('[data-testid="validation-warning"]')).toBeVisible();
        await expect(page.locator('[data-testid="validation-warning"]')).toContainText('TAN value seems unusually high');
    });

    test('should clear data when clear button is clicked', async ({ page }) => {
        // Navigate to TAN test entry and enter data
        await page.click('[data-testid="test-selection"]');
        await page.selectOption('[data-testid="test-dropdown"]', { label: 'TAN by Color Indication' });
        await page.selectOption('[data-testid="sample-dropdown"]', { index: 1 });

        await page.fill('[data-testid="trial-1-sample-weight"]', '10.0');
        await page.fill('[data-testid="trial-1-final-buret"]', '5.5');
        await page.fill('[data-testid="main-comments"]', 'Test comment');

        // Click clear button
        await page.click('[data-testid="clear-button"]');

        // Confirm in dialog
        await page.click('[data-testid="confirm-clear"]');

        // Verify data is cleared
        await expect(page.locator('[data-testid="trial-1-sample-weight"]')).toHaveValue('');
        await expect(page.locator('[data-testid="trial-1-final-buret"]')).toHaveValue('');
        await expect(page.locator('[data-testid="trial-1-tan-result"]')).toHaveValue('');
        await expect(page.locator('[data-testid="main-comments"]')).toHaveValue('');
    });

    test('should show historical results', async ({ page }) => {
        // Navigate to TAN test entry
        await page.click('[data-testid="test-selection"]');
        await page.selectOption('[data-testid="test-dropdown"]', { label: 'TAN by Color Indication' });
        await page.selectOption('[data-testid="sample-dropdown"]', { index: 1 });

        // Verify historical results panel is visible
        await expect(page.locator('[data-testid="historical-results"]')).toBeVisible();
        await expect(page.locator('[data-testid="historical-results-title"]')).toContainText('Last 12 results for TAN by Color Indication');

        // Verify historical data is displayed
        const historyRows = page.locator('[data-testid="history-row"]');
        await expect(historyRows).toHaveCount.greaterThan(0);

        // Test resizable functionality
        const resizeHandle = page.locator('[data-testid="resize-handle"]');
        await resizeHandle.hover();
        await page.mouse.down();
        await page.mouse.move(100, 0);
        await page.mouse.up();

        // Verify panel was resized
        const panel = page.locator('[data-testid="historical-results"]');
        const boundingBox = await panel.boundingBox();
        expect(boundingBox?.width).toBeGreaterThan(300);
    });

    test('should support keyboard shortcuts', async ({ page }) => {
        // Navigate to TAN test entry and enter data
        await page.click('[data-testid="test-selection"]');
        await page.selectOption('[data-testid="test-dropdown"]', { label: 'TAN by Color Indication' });
        await page.selectOption('[data-testid="sample-dropdown"]', { index: 1 });

        await page.fill('[data-testid="trial-1-sample-weight"]', '10.0');
        await page.fill('[data-testid="trial-1-final-buret"]', '5.5');

        // Test Ctrl+S for save
        await page.keyboard.press('Control+s');
        await expect(page.locator('[data-testid="success-message"]')).toBeVisible();

        // Test Ctrl+R for clear (after entering new data)
        await page.fill('[data-testid="trial-2-sample-weight"]', '11.0');
        await page.keyboard.press('Control+r');
        await page.click('[data-testid="confirm-clear"]');

        // Verify data is cleared
        await expect(page.locator('[data-testid="trial-2-sample-weight"]')).toHaveValue('');
    });
});