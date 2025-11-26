import { test, expect } from '@playwright/test';

test.describe('File Upload Workflow', () => {
    test.beforeEach(async ({ page }) => {
        await page.goto('/');
        await page.waitForLoadState('networkidle');
    });

    test('should upload and preview files', async ({ page }) => {
        // Navigate to a test that supports file upload
        await page.click('[data-testid="test-selection"]');
        await page.selectOption('[data-testid="test-dropdown"]', { label: 'Water-KF' });
        await page.selectOption('[data-testid="sample-dropdown"]', { index: 1 });

        // Upload a file
        const fileInput = page.locator('[data-testid="file-upload-trial-1"]');
        await fileInput.setInputFiles('test-data/water-kf-data.dat');

        // Verify file is uploaded and preview is available
        await expect(page.locator('[data-testid="uploaded-file-name"]')).toContainText('water-kf-data.dat');
        await expect(page.locator('[data-testid="file-preview-button"]')).toBeVisible();

        // Test file preview
        await page.click('[data-testid="file-preview-button"]');
        await expect(page.locator('[data-testid="file-preview-dialog"]')).toBeVisible();
        await expect(page.locator('[data-testid="file-content"]')).toContainText('Water Content');

        // Close preview
        await page.click('[data-testid="close-preview"]');
        await expect(page.locator('[data-testid="file-preview-dialog"]')).not.toBeVisible();
    });

    test('should validate file types and sizes', async ({ page }) => {
        await page.click('[data-testid="test-selection"]');
        await page.selectOption('[data-testid="test-dropdown"]', { label: 'Water-KF' });
        await page.selectOption('[data-testid="sample-dropdown"]', { index: 1 });

        // Try to upload invalid file type
        const fileInput = page.locator('[data-testid="file-upload-trial-1"]');
        await fileInput.setInputFiles('test-data/invalid-file.exe');

        // Verify error message
        await expect(page.locator('[data-testid="file-error"]')).toContainText('Invalid file type');

        // Try to upload oversized file
        await fileInput.setInputFiles('test-data/large-file.dat');
        await expect(page.locator('[data-testid="file-error"]')).toContainText('File size exceeds limit');
    });

    test('should support drag and drop file upload', async ({ page }) => {
        await page.click('[data-testid="test-selection"]');
        await page.selectOption('[data-testid="test-dropdown"]', { label: 'RBOT' });
        await page.selectOption('[data-testid="sample-dropdown"]', { index: 1 });

        // Simulate drag and drop
        const dropZone = page.locator('[data-testid="file-drop-zone"]');

        // Create a file for drag and drop simulation
        const dataTransfer = await page.evaluateHandle(() => new DataTransfer());

        await dropZone.dispatchEvent('dragover', { dataTransfer });
        await expect(dropZone).toHaveClass(/drag-over/);

        await dropZone.dispatchEvent('drop', { dataTransfer });

        // Verify file upload processing
        await expect(page.locator('[data-testid="upload-progress"]')).toBeVisible();
    });
});