# Budget Platform

Enterprise Budget Management Platform built with .NET 8, ASP.NET Core, PostgreSQL, React, and designed for Microsoft Teams integration.

## Features (MVP - Phase 1)

- **Excel Import Pipeline**: Upload XLSX → Preview parsed rows → Commit to PostgreSQL
- **Data Explorer**: List budget requests with filters, search, and detail views
- **Excel Export**: Generate Excel exports per request
- **Admin Management**: Manage dropdown dimensions and dynamic field schemas
- **Audit Trail**: Track who/when/what for all changes
- **Teams Tab Ready**: Route prepared for Microsoft Teams integration
- **React Web UI**: Minimal MVP interface for all operations

## Tech Stack

| Layer | Technology |
|-------|------------|
| Frontend | React 18, TypeScript, Vite |
| Backend | C# .NET 8, ASP.NET Core Web API |
| Database | PostgreSQL via EF Core (Npgsql) |
| Excel Parsing | ClosedXML |
| Validation | FluentValidation |
| CQRS | MediatR |
| Logging | Serilog |
| Errors | ProblemDetails (RFC7807) |
| Auth | Entra ID JWT (with dev fallback) |
| API Docs | Swagger/OpenAPI |

## Project Structure

```
budget-platform/
├── src/
│   ├── BudgetPlatform.sln
│   ├── Budget.Api/              # ASP.NET Core host, controllers, middleware
│   ├── Budget.Core/             # Domain entities, application layer (CQRS)
│   ├── Budget.Infrastructure/   # EF Core, repositories, file storage, Excel
│   └── web/                     # React + TypeScript web UI
│       ├── src/
│       │   ├── api/             # API client
│       │   ├── auth/            # Auth context with Teams integration
│       │   ├── components/      # Shared components
│       │   └── pages/           # Page components
│       ├── package.json
│       └── vite.config.ts
├── tests/
│   ├── Budget.Tests.Unit/       # Unit tests with xUnit, Moq
│   └── Budget.Tests.Integration/# Integration tests with Testcontainers
├── docker/
│   └── compose.dev.yml          # PostgreSQL dev container
├── tools/db/
│   ├── migrate.ps1              # Run EF migrations
│   └── seed.ps1                 # Seed initial data
└── README.md
```

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Node.js 18+](https://nodejs.org/) (for web UI)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/)
- [PowerShell](https://docs.microsoft.com/en-us/powershell/) (for scripts)

## Quick Start

### 1. Start PostgreSQL

```powershell
cd docker
docker compose -f compose.dev.yml up -d
```

This starts PostgreSQL on `localhost:5432` with:
- Database: `budget_platform`
- User: `budget_user`
- Password: `budget_pass`

### 2. Run the API

```powershell
cd src/Budget.Api
dotnet run
```

The API starts at:
- HTTP: `http://localhost:5000`
- HTTPS: `https://localhost:5001`
- Swagger: `https://localhost:5001/swagger`

Migrations run automatically in development mode.

### 3. Run the Web UI

```powershell
cd src/web
npm install
npm run dev
```

The UI starts at `http://localhost:3000` and proxies API requests to the backend.

### 4. Seed Initial Data (Optional)

```powershell
.\tools\db\seed.ps1
```

## Web UI Routes

| Route | Description |
|-------|-------------|
| `/teams` | Landing page with user info and quick links |
| `/imports` | Upload Excel file for import |
| `/imports/:id/preview` | Preview parsed data, validation errors, commit |
| `/requests` | List budget requests with filters & search |
| `/requests/:id` | Request details with export button |

## API Endpoints

### Import Pipeline

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/imports/upload` | Upload Excel file (multipart) |
| GET | `/api/imports/{id}/preview` | Get parsed preview with validation |
| POST | `/api/imports/{id}/commit` | Commit import to database |

### Budget Requests

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/requests` | List with filters & pagination |
| GET | `/api/requests/{id}` | Get request details |
| GET | `/api/requests/{id}/export` | Export as Excel |

### Admin

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/admin/field-schema` | Get all field schemas |
| PUT | `/api/admin/field-schema` | Update field schemas |
| GET | `/api/admin/dimensions` | Get all dimension keys |
| GET | `/api/admin/dimensions/{enumKey}` | Get dimension values |
| PUT | `/api/admin/dimensions/{enumKey}` | Update dimension values |

### Health & Teams

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/health` | Health check |
| GET | `/ready` | Readiness check |
| GET | `/teams` | Teams Tab route |

## Configuration

### Backend - appsettings.Development.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=budget_platform;Username=budget_user;Password=budget_pass"
  },
  "Auth": {
    "UseDevAuth": true,
    "DevUserId": "dev-user-001",
    "DevUserName": "Local Developer"
  }
}
```

### Frontend - Environment Variables

Create `.env` file in `src/web/`:

```env
VITE_API_BASE_URL=/api
VITE_DEV_AUTH=true
```

### Production (appsettings.json)

Configure Entra ID authentication:

```json
{
  "Auth": {
    "UseDevAuth": false
  },
  "AzureAd": {
    "Instance": "https://login.microsoftonline.com/",
    "TenantId": "YOUR_TENANT_ID",
    "ClientId": "YOUR_CLIENT_ID",
    "Audience": "api://YOUR_CLIENT_ID"
  }
}
```

### Excel Parser Configuration

Customize Excel parsing positions in `appsettings.json`:

```json
{
  "ExcelParser": {
    "SheetName": "Budget",
    "Header": {
      "CellMappings": {
        "Title": "B2",
        "RequestNumber": "B3",
        "FiscalYear": "B9"
      }
    },
    "Detail": {
      "StartRow": 14,
      "ColumnMappings": {
        "LineDescription": "A",
        "Amount": "F"
      }
    }
  }
}
```

## Development

### Run Tests

```powershell
# Unit tests
dotnet test tests/Budget.Tests.Unit

# Integration tests (requires Docker)
dotnet test tests/Budget.Tests.Integration
```

### Add New Migration

```powershell
cd src/Budget.Infrastructure
dotnet ef migrations add MigrationName --startup-project ../Budget.Api/Budget.Api.csproj
```

### Build Web UI for Production

```powershell
cd src/web
npm run build
```

Output goes to `src/web/dist/`.

### pgAdmin (Optional)

Start pgAdmin for database management:

```powershell
docker compose -f docker/compose.dev.yml --profile tools up -d
```

Access at `http://localhost:5050` (admin@budget.local / admin123)

## Teams Integration

The web UI includes Teams SDK integration via `@microsoft/teams-js`:

1. **Inside Teams**: Automatically initializes Teams SDK and gets auth token
2. **Outside Teams (dev)**: Falls back to mock dev user

### Teams App Deployment

The `teams/` folder contains the manifest and deployment scripts:

```powershell
# Configure environment
cp teams/env.template teams/.env.local
# Edit .env.local with your Azure AD app details

# Package for deployment
.\teams\package-manifest.ps1 -Environment prod
```

See [teams/README.md](teams/README.md) for detailed setup instructions.

## CI/CD

GitHub Actions workflows in `.github/workflows/`:

| Workflow | Trigger | Description |
|----------|---------|-------------|
| `ci.yml` | Push/PR | Build, test, lint, security scan |
| `cd.yml` | Push to main, tags | Deploy to staging/production |

### CI Pipeline

- Builds backend (.NET 8)
- Runs unit tests
- Runs integration tests (with PostgreSQL)
- Builds frontend (Vite)
- Runs ESLint
- Security vulnerability scan

### CD Pipeline

- **Staging**: Auto-deploys on push to `main`
- **Production**: Deploys on version tags (`v1.0.0`)
- **Teams Package**: Creates Teams app ZIP on release

## Git Workflow

Helper scripts in `tools/git/`:

```powershell
# Set up pre-commit and pre-push hooks
.\tools\git\setup-hooks.ps1

# Create a new feature branch
.\tools\git\new-feature.ps1 -Name "add export filter" -Type feature

# Check if ready for PR
.\tools\git\pr-ready.ps1

# Create a release
.\tools\git\create-release.ps1 -Version "1.0.0"
```

### Branch Strategy

- `main` - Production-ready code
- `develop` - Integration branch (optional)
- `feature/*` - New features
- `bugfix/*` - Bug fixes
- `hotfix/*` - Production hotfixes

## Domain Model

### Core Entities

- **BudgetRequest**: Header-level budget with dimensions and status
- **BudgetItem**: Line items with amounts and monthly breakdown
- **BudgetSection**: Grouping/hierarchy for items
- **ImportRun**: Tracks file upload and parsing status
- **FieldSchema**: Dynamic field definitions
- **DimensionValue**: Lookup values for dropdowns
- **AuditLog**: Change tracking

### Dimension Types

Pre-configured dimensions: `channel`, `owner`, `frequency`, `vendor`

## License

Proprietary - Internal Use Only
