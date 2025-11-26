import { test, expect, devices } from '@playwright/test';

test.describe('Cross-Browser Compatibility', () => {
    const browsers = ['chromium', 'firefox', 'webkit'];

    browsers.forEach(browserName => {
        test.describe(`${browserName} compatibility`, () => {
            test.use({
                ...devices[browserName === 'webkit' ? 'Desktop Safari' :
                    browserName === 'firefox' ? 'Desktop Firefox' : 'Desktop Chrome']
            });

            test('should load application correctly', async ({ page }) => {
                await page.goto('/');

                // Verify main elements are visible
                await expect(page.locator('[data-testid="app-header"]')).toBeVisible();
                await expect(page.locator('[data-testid="test-selection"]')).toBeVisible();
                await expect(page.locator('[data-testid="sample-selection"]')).toBeVisible();
            });

            test('should handle form interactions', async ({ page }) => {
                await page.goto('/');

                // Test dropdown interactions
                await page.click('[data-testid="test-selection"]');
                await page.selectOption('[data-testid="test-dropdown"]', { label: 'TAN by Color Indication' });

                // Test input fields
                await page.selectOption('[data-testid="sample-dropdown"]', { index: 1 });
                await page.fill('[data-testid="trial-1-sample-weight"]', '10.0');

                // Verify input is accepted
                await expect(page.locator('[data-testid="trial-1-sample-weight"]')).toHaveValue('10.0');
            });

            test('should support keyboard navigation', async ({ page }) => {
                await page.goto('/');

                // Test tab navigation
                await page.keyboard.press('Tab');
                await page.keyboard.press('Tab');

                // Verify focus is on expected element
                const focusedElement = await page.locator(':focus');
                await expect(focusedElement).toBeVisible();
            });

            test('should handle responsive design', async ({ page }) => {
                await page.goto('/');

                // Test mobile viewport
                await page.setViewportSize({ width: 375, height: 667 });

                // Verify mobile layout
                await expect(page.locator('[data-testid="mobile-menu"]')).toBeVisible();

                // Test tablet viewport
                await page.setViewportSize({ width: 768, height: 1024 });

                // Verify tablet layout adjustments
                await expect(page.locator('[data-testid="main-content"]')).toBeVisible();

                // Test desktop viewport
                await page.setViewportSize({ width: 1920, height: 1080 });

                // Verify desktop layout
                await expect(page.locator('[data-testid="desktop-layout"]')).toBeVisible();
            });
        });
    });

    test('should maintain functionality across all browsers', async ({ page, browserName }) => {
        await page.goto('/');

        // Complete a full workflow in each browser
        await page.click('[data-testid="test-selection"]');
        await page.selectOption('[data-testid="test-dropdown"]', { label: 'TAN by Color Indication' });
        await page.selectOption('[data-testid="sample-dropdown"]', { index: 1 });

        await page.fill('[data-testid="trial-1-sample-weight"]', '10.0');
        await page.fill('[data-testid="trial-1-final-buret"]', '5.5');

        // Verify calculation works in all browsers
        await expect(page.locator('[data-testid="trial-1-tan-result"]')).toHaveValue('3.08');

        // Test save functionality
        await page.click('[data-testid="save-button"]');
        await expect(page.locator('[data-testid="success-message"]')).toBeVisible();
    });
});