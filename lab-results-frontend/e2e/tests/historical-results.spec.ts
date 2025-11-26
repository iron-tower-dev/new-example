import { test, expect } from '@playwright/test';

test.describe('Historical Results Functionality', () => {
    test.beforeEach(async ({ page }) => {
        await page.goto('/');
        await page.waitForLoadState('networkidle');
    });

    test('should display historical results panel', async ({ page }) => {
        // Navigate to any test entry
        await page.click('[data-testid="test-selection"]');
        await page.selectOption('[data-testid="test-dropdown"]', { label: 'TAN by Color Indication' });
        await page.selectOption('[data-testid="sample-dropdown"]', { index: 1 });

        // Verify historical results panel is visible
        await expect(page.locator('[data-testid="historical-results-panel"]')).toBeVisible();
        await expect(page.locator('[data-testid="historical-results-title"]')).toContainText('Last 12 results for TAN by Color Indication');

        // Verify historical data is displayed
        const historyRows = page.locator('[data-testid="history-row"]');
        await expect(historyRows.first()).toBeVisible();

        // Verify columns are present
        await expect(page.locator('[data-testid="history-header-date"]')).toContainText('Sample Date');
        await expect(page.locator('[data-testid="history-header-sample"]')).toContainText('Sample ID');
        await expect(page.locator('[data-testid="history-header-result"]')).toContainText('Result');
    });

    test('should support resizable historical results panel', async ({ page }) => {
        await page.click('[data-testid="test-selection"]');
        await page.selectOption('[data-testid="test-dropdown"]', { label: 'TAN by Color Indication' });
        await page.selectOption('[data-testid="sample-dropdown"]', { index: 1 });

        // Get initial panel width
        const panel = page.locator('[data-testid="historical-results-panel"]');
        const initialBox = await panel.boundingBox();

        // Resize the panel
        const resizeHandle = page.locator('[data-testid="resize-handle"]');
        await resizeHandle.hover();
        await page.mouse.down();
        await page.mouse.move(100, 0);
        await page.mouse.up();

        // Verify panel was resized
        const newBox = await panel.boundingBox();
        expect(newBox?.width).toBeGreaterThan(initialBox?.width || 0);
    });

    test('should support single screen mode toggle', async ({ page }) => {
        await page.click('[data-testid="test-selection"]');
        await page.selectOption('[data-testid="test-dropdown"]', { label: 'TAN by Color Indication' });
        await page.selectOption('[data-testid="sample-dropdown"]', { index: 1 });

        // Click single screen mode button
        await page.click('[data-testid="single-screen-mode"]');

        // Verify historical results takes full screen
        const panel = page.locator('[data-testid="historical-results-panel"]');
        await expect(panel).toHaveClass(/full-screen/);

        // Verify test entry form is hidden
        await expect(page.locator('[data-testid="test-entry-form"]')).not.toBeVisible();

        // Toggle back to split view
        await page.click('[data-testid="split-screen-mode"]');
        await expect(panel).not.toHaveClass(/full-screen/);
        await expect(page.locator('[data-testid="test-entry-form"]')).toBeVisible();
    });

    test('should show detailed historical result on click', async ({ page }) => {
        await page.click('[data-testid="test-selection"]');
        await page.selectOption('[data-testid="test-dropdown"]', { label: 'TAN by Color Indication' });
        await page.selectOption('[data-testid="sample-dropdown"]', { index: 1 });

        // Click on a historical result row
        await page.click('[data-testid="history-row"]:first-child');

        // Verify detailed dialog opens
        await expect(page.locator('[data-testid="historical-detail-dialog"]')).toBeVisible();
        await expect(page.locator('[data-testid="detail-sample-info"]')).toBeVisible();
        await expect(page.locator('[data-testid="detail-trial-data"]')).toBeVisible();
        await expect(page.locator('[data-testid="detail-comments"]')).toBeVisible();

        // Close dialog
        await page.click('[data-testid="close-detail-dialog"]');
        await expect(page.locator('[data-testid="historical-detail-dialog"]')).not.toBeVisible();
    });

    test('should support extended history access', async ({ page }) => {
        await page.click('[data-testid="test-selection"]');
        await page.selectOption('[data-testid="test-dropdown"]', { label: 'TAN by Color Indication' });
        await page.selectOption('[data-testid="sample-dropdown"]', { index: 1 });

        // Click extended history button
        await page.click('[data-testid="extended-history-button"]');

        // Verify extended history dialog opens
        await expect(page.locator('[data-testid="extended-history-dialog"]')).toBeVisible();

        // Test date range filtering
        await page.fill('[data-testid="history-from-date"]', '2024-01-01');
        await page.fill('[data-testid="history-to-date"]', '2024-12-31');
        await page.click('[data-testid="apply-date-filter"]');

        // Verify filtered results
        const filteredRows = page.locator('[data-testid="extended-history-row"]');
        await expect(filteredRows.first()).toBeVisible();

        // Test pagination
        await page.click('[data-testid="next-page"]');
        await expect(page.locator('[data-testid="page-indicator"]')).toContainText('Page 2');
    });

    test('should update historical results when sample changes', async ({ page }) => {
        await page.click('[data-testid="test-selection"]');
        await page.selectOption('[data-testid="test-dropdown"]', { label: 'TAN by Color Indication' });

        // Select first sample
        await page.selectOption('[data-testid="sample-dropdown"]', { index: 1 });

        // Get initial history data
        const firstSampleHistory = await page.locator('[data-testid="history-row"]:first-child [data-testid="sample-id"]').textContent();

        // Select different sample
        await page.selectOption('[data-testid="sample-dropdown"]', { index: 2 });

        // Verify history updated for new sample
        const secondSampleHistory = await page.locator('[data-testid="history-row"]:first-child [data-testid="sample-id"]').textContent();
        expect(secondSampleHistory).not.toBe(firstSampleHistory);
    });
});