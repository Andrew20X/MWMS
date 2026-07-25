import { test, expect } from '@playwright/test';

test.describe('Leaves Flows', () => {
  test.beforeEach(async ({ page }) => {
    // Navigate and login
    await page.goto('http://localhost:5173/');
    
    // We assume the user logs in as Admin
    await page.fill('input[type="text"]', 'admin');
    await page.fill('input[type="password"]', 'Password123!');
    await page.click('button[type="submit"]');

    // Wait for dashboard to load (the root path /)
    await expect(page).toHaveURL(/.*#\//);
  });

  test('should load leaves page and display table', async ({ page }) => {
    // Navigate to leaves page
    await page.click('text=Leaves');
    await expect(page).toHaveURL(/.*leaves/);
    
    // Check if the leaves table is present
    await expect(page.locator('table')).toBeVisible();
    
    // Check if "All Leave Requests" or similar title is there (Admin view)
    await expect(page.locator('text=All Leave Requests')).toBeVisible();
  });
  
  test('should open add leave dialog', async ({ page }) => {
    await page.click('text=Leaves');
    await expect(page).toHaveURL(/.*leaves/);

    // Click "Request Leave" button
    await page.click('button:has-text("Request Leave")');

    // Check if dialog is visible
    await expect(page.locator('text=Request Leave')).toBeVisible();
    await expect(page.locator('button:has-text("Cancel")')).toBeVisible();
  });
});
