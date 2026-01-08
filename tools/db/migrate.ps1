<#
.SYNOPSIS
    Runs EF Core migrations for the Budget Platform database.

.DESCRIPTION
    This script applies pending migrations to the PostgreSQL database.
    Make sure Docker postgres container is running before executing.

.PARAMETER Environment
    The environment to run migrations for (Development, Staging, Production).
    Default: Development

.EXAMPLE
    .\migrate.ps1
    .\migrate.ps1 -Environment Production
#>

param(
    [string]$Environment = "Development"
)

$ErrorActionPreference = "Stop"

Write-Host "Running migrations for environment: $Environment" -ForegroundColor Cyan

# Navigate to the infrastructure project
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$infraDir = Join-Path $scriptDir "..\..\src\Budget.Infrastructure"

Push-Location $infraDir

try {
    # Set environment variable
    $env:ASPNETCORE_ENVIRONMENT = $Environment

    Write-Host "Applying migrations..." -ForegroundColor Yellow
    dotnet ef database update --startup-project ..\Budget.Api\Budget.Api.csproj

    if ($LASTEXITCODE -eq 0) {
        Write-Host "Migrations applied successfully!" -ForegroundColor Green
    } else {
        Write-Host "Migration failed with exit code: $LASTEXITCODE" -ForegroundColor Red
        exit $LASTEXITCODE
    }
}
finally {
    Pop-Location
}

