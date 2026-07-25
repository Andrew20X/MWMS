import { test, expect } from '@playwright/test';

test.describe('Authentication Flows', () => {
  test.beforeEach(async ({ page }) => {
    // Navigate to the app (assuming it runs on port 5173 locally)
    await page.goto('http://localhost:5173/');
  });

  test('should display login page when not authenticated', async ({ page }) => {
    await expect(page).toHaveTitle(/MWMS/);
    await expect(page.locator('text=Sign in to your account')).toBeVisible();
    await expect(page.locator('button[type="submit"]')).toBeVisible();
  });

  test('should show error on invalid login', async ({ page }) => {
    await page.fill('input[type="text"]', 'invalid_user');
    await page.fill('input[type="password"]', 'wrongpassword');
    await page.click('button[type="submit"]');

    const errorToast = page.locator('.MuiAlert-message');
    await expect(errorToast).toBeVisible();
    await expect(errorToast).toContainText('Invalid username or password.');
  });
});
