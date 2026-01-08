# Teams App Configuration

This folder contains the Microsoft Teams app manifest and configuration.

## Files

| File | Description |
|------|-------------|
| `manifest.json` | Teams app manifest template with placeholders |
| `color.png` | 192x192 color icon (replace placeholder) |
| `outline.png` | 32x32 outline icon (replace placeholder) |
| `env.template` | Environment variables template |
| `package-manifest.ps1` | Script to package manifest for deployment |

## Setup

### 1. Azure AD App Registration

1. Go to [Azure Portal](https://portal.azure.com) > Azure Active Directory > App registrations
2. Click "New registration"
3. Configure:
   - Name: `Budget Platform`
   - Supported account types: Accounts in this organizational directory only
   - Redirect URI: `https://YOUR_HOSTNAME/auth/callback`
4. Note the **Application (client) ID** and **Directory (tenant) ID**
5. Under "Certificates & secrets", create a new client secret
6. Under "API permissions", add:
   - `User.Read` (Delegated)
7. Under "Expose an API":
   - Set Application ID URI: `api://YOUR_HOSTNAME/CLIENT_ID`
   - Add scope: `access_as_user`

### 2. Configure Environment

Copy `env.template` to `.env.local` and fill in:

```env
AAD_APP_CLIENT_ID=your-client-id
AAD_APP_CLIENT_SECRET=your-client-secret
AAD_APP_TENANT_ID=your-tenant-id
PUBLIC_HOSTNAME=budget-platform.azurewebsites.net
APP_ID=generate-new-guid
```

### 3. Create Icons

Replace the placeholder files with actual PNG icons:

**color.png** (192x192):
- Full color logo
- Used in app listings and headers

**outline.png** (32x32):
- Single color (white) outline
- Transparent background
- Used in compact spaces

### 4. Package Manifest

```powershell
.\package-manifest.ps1 -Environment prod
```

This creates `budget-platform-teams-prod.zip` ready for upload.

## Deployment Options

### Option A: Teams Admin Center

1. Go to [Teams Admin Center](https://admin.teams.microsoft.com)
2. Navigate to Teams apps > Manage apps
3. Click "Upload new app"
4. Select the ZIP file

### Option B: Teams Developer Portal

1. Go to [Teams Developer Portal](https://dev.teams.microsoft.com)
2. Click "Apps" > "Import app"
3. Select the ZIP file
4. Configure and publish

### Option C: Side-loading (Development)

1. In Teams, click "Apps" > "Manage your apps"
2. Click "Upload an app"
3. Select "Upload a custom app"
4. Choose the ZIP file

## Manifest Placeholders

| Placeholder | Description | Example |
|------------|-------------|---------|
| `{{APP_ID}}` | Teams app GUID | `12345678-1234-...` |
| `{{AAD_APP_CLIENT_ID}}` | Azure AD app ID | `abcdef12-...` |
| `{{PUBLIC_HOSTNAME}}` | Deployed app URL | `budget-platform.azurewebsites.net` |

## Tab Configuration

The manifest configures three static tabs:

1. **Home** (`/teams`) - Landing page with user info
2. **Import** (`/imports`) - Upload budget files
3. **Requests** (`/requests`) - View and manage requests

## Troubleshooting

### App not loading in Teams

1. Check browser console for errors
2. Verify `validDomains` includes your hostname
3. Ensure HTTPS is configured correctly
4. Check CORS settings in the API

### Authentication issues

1. Verify Azure AD app registration
2. Check `webApplicationInfo` in manifest
3. Ensure redirect URIs are correct
4. Test SSO flow in Teams Toolkit

### "App not found" error

1. App may not be approved yet
2. Check if app is enabled in Teams Admin Center
3. Try side-loading for testing

