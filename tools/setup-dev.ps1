<#
.SYNOPSIS
    Sets up the complete development environment for Budget Platform.

.DESCRIPTION
    This script automates the entire development setup process:
    - Verifies prerequisites
    - Starts Docker containers
    - Restores dependencies
    - Applies database migrations
    - Sets up Git hooks

.EXAMPLE
    .\tools\setup-dev.ps1
#>

$ErrorActionPreference = "Stop"

Write-Host ""
Write-Host "============================================" -ForegroundColor Cyan
Write-Host "  Budget Platform - Development Setup" -ForegroundColor Cyan
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""

$repoRoot = Split-Path -Parent (Split-Path -Parent $MyInvocation.MyCommand.Path)
Set-Location $repoRoot

# Check prerequisites
Write-Host "[1/7] Checking prerequisites..." -ForegroundColor Yellow

$prereqs = @{
    "dotnet" = "dotnet --version"
    "node" = "node --version"
    "npm" = "npm --version"
    "docker" = "docker --version"
}

$allPrereqs = $true
foreach ($tool in $prereqs.Keys) {
    try {
        $version = Invoke-Expression $prereqs[$tool] 2>$null
        Write-Host "  ✓ $tool : $version" -ForegroundColor Green
    }
    catch {
        Write-Host "  ✗ $tool : NOT FOUND" -ForegroundColor Red
        $allPrereqs = $false
    }
}

if (-not $allPrereqs) {
    Write-Host ""
    Write-Host "Please install missing prerequisites and try again." -ForegroundColor Red
    exit 1
}

# Start Docker containers
Write-Host ""
Write-Host "[2/7] Starting Docker containers..." -ForegroundColor Yellow
docker compose -f docker/compose.dev.yml up -d
if ($LASTEXITCODE -ne 0) {
    Write-Host "  Failed to start Docker containers" -ForegroundColor Red
    exit 1
}
Write-Host "  ✓ PostgreSQL running on localhost:5432" -ForegroundColor Green

# Wait for PostgreSQL to be ready
Write-Host ""
Write-Host "[3/7] Waiting for PostgreSQL..." -ForegroundColor Yellow
$maxAttempts = 30
$attempt = 0
do {
    $attempt++
    $ready = docker exec budget-postgres pg_isready -U budget_user -d budget_platform 2>$null
    if ($LASTEXITCODE -eq 0) {
        Write-Host "  ✓ PostgreSQL is ready" -ForegroundColor Green
        break
    }
    Start-Sleep -Seconds 1
} while ($attempt -lt $maxAttempts)

if ($attempt -eq $maxAttempts) {
    Write-Host "  PostgreSQL failed to start in time" -ForegroundColor Red
    exit 1
}

# Restore .NET dependencies
Write-Host ""
Write-Host "[4/7] Restoring .NET dependencies..." -ForegroundColor Yellow
dotnet restore src/BudgetPlatform.sln --verbosity minimal
if ($LASTEXITCODE -ne 0) {
    Write-Host "  Failed to restore .NET dependencies" -ForegroundColor Red
    exit 1
}
Write-Host "  ✓ .NET dependencies restored" -ForegroundColor Green

# Apply database migrations
Write-Host ""
Write-Host "[5/7] Applying database migrations..." -ForegroundColor Yellow
Push-Location src/Budget.Infrastructure
dotnet ef database update --startup-project ../Budget.Api/Budget.Api.csproj --verbosity minimal
$migrationResult = $LASTEXITCODE
Pop-Location

if ($migrationResult -ne 0) {
    Write-Host "  Failed to apply migrations" -ForegroundColor Red
    exit 1
}
Write-Host "  ✓ Database migrations applied" -ForegroundColor Green

# Install npm dependencies
Write-Host ""
Write-Host "[6/7] Installing npm dependencies..." -ForegroundColor Yellow
Push-Location src/web
npm install --silent
$npmResult = $LASTEXITCODE
Pop-Location

if ($npmResult -ne 0) {
    Write-Host "  Failed to install npm dependencies" -ForegroundColor Red
    exit 1
}
Write-Host "  ✓ npm dependencies installed" -ForegroundColor Green

# Setup Git hooks
Write-Host ""
Write-Host "[7/7] Setting up Git hooks..." -ForegroundColor Yellow
& .\tools\git\setup-hooks.ps1 2>$null
Write-Host "  ✓ Git hooks installed" -ForegroundColor Green

# Done!
Write-Host ""
Write-Host "============================================" -ForegroundColor Green
Write-Host "  Setup Complete!" -ForegroundColor Green
Write-Host "============================================" -ForegroundColor Green
Write-Host ""
Write-Host "To start developing:" -ForegroundColor Cyan
Write-Host ""
Write-Host "  Terminal 1 - Run API:" -ForegroundColor White
Write-Host "    cd src/Budget.Api" -ForegroundColor Gray
Write-Host "    dotnet run" -ForegroundColor Gray
Write-Host ""
Write-Host "  Terminal 2 - Run Web UI:" -ForegroundColor White
Write-Host "    cd src/web" -ForegroundColor Gray
Write-Host "    npm run dev" -ForegroundColor Gray
Write-Host ""
Write-Host "  Access:" -ForegroundColor White
Write-Host "    API:     https://localhost:5001" -ForegroundColor Gray
Write-Host "    Swagger: https://localhost:5001/swagger" -ForegroundColor Gray
Write-Host "    Web UI:  http://localhost:3000" -ForegroundColor Gray
Write-Host ""

