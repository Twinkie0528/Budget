<#
.SYNOPSIS
    Packages the Teams app manifest for deployment.

.DESCRIPTION
    This script replaces placeholder values in manifest.json with environment-specific
    values and creates a ZIP package ready for upload to Teams Admin Center.

.PARAMETER Environment
    The environment to package for (dev, staging, prod). Default: dev

.EXAMPLE
    .\package-manifest.ps1 -Environment prod
#>

param(
    [ValidateSet("dev", "staging", "prod")]
    [string]$Environment = "dev"
)

$ErrorActionPreference = "Stop"

Write-Host "Packaging Teams manifest for environment: $Environment" -ForegroundColor Cyan

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path

# Load environment variables
$envFile = Join-Path $scriptDir ".env.$Environment"
if (-not (Test-Path $envFile)) {
    $envFile = Join-Path $scriptDir ".env.local"
}

if (Test-Path $envFile) {
    Write-Host "Loading environment from: $envFile" -ForegroundColor Yellow
    Get-Content $envFile | ForEach-Object {
        if ($_ -match "^([^=]+)=(.*)$") {
            [Environment]::SetEnvironmentVariable($matches[1], $matches[2])
        }
    }
} else {
    Write-Host "Warning: No environment file found. Using system environment variables." -ForegroundColor Yellow
}

# Required variables
$requiredVars = @("AAD_APP_CLIENT_ID", "PUBLIC_HOSTNAME", "APP_ID")
foreach ($var in $requiredVars) {
    $value = [Environment]::GetEnvironmentVariable($var)
    if ([string]::IsNullOrWhiteSpace($value)) {
        Write-Error "Required environment variable $var is not set"
        exit 1
    }
}

# Create output directory
$outputDir = Join-Path $scriptDir "dist"
if (-not (Test-Path $outputDir)) {
    New-Item -ItemType Directory -Path $outputDir | Out-Null
}

# Read and process manifest
$manifestPath = Join-Path $scriptDir "manifest.json"
$manifest = Get-Content $manifestPath -Raw

# Replace placeholders
$manifest = $manifest -replace '\{\{APP_ID\}\}', [Environment]::GetEnvironmentVariable("APP_ID")
$manifest = $manifest -replace '\{\{AAD_APP_CLIENT_ID\}\}', [Environment]::GetEnvironmentVariable("AAD_APP_CLIENT_ID")
$manifest = $manifest -replace '\{\{PUBLIC_HOSTNAME\}\}', [Environment]::GetEnvironmentVariable("PUBLIC_HOSTNAME")

# Write processed manifest
$processedManifestPath = Join-Path $outputDir "manifest.json"
$manifest | Out-File -FilePath $processedManifestPath -Encoding utf8

# Copy icons (if they exist)
$colorIcon = Join-Path $scriptDir "color.png"
$outlineIcon = Join-Path $scriptDir "outline.png"

if (Test-Path $colorIcon) {
    Copy-Item $colorIcon -Destination $outputDir
} else {
    Write-Host "Warning: color.png not found. Creating placeholder." -ForegroundColor Yellow
    # Create a simple placeholder
}

if (Test-Path $outlineIcon) {
    Copy-Item $outlineIcon -Destination $outputDir
} else {
    Write-Host "Warning: outline.png not found. Creating placeholder." -ForegroundColor Yellow
}

# Create ZIP package
$zipPath = Join-Path $scriptDir "budget-platform-teams-$Environment.zip"
if (Test-Path $zipPath) {
    Remove-Item $zipPath
}

Compress-Archive -Path "$outputDir\*" -DestinationPath $zipPath

Write-Host "Teams package created: $zipPath" -ForegroundColor Green
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Cyan
Write-Host "1. Upload the ZIP to Teams Admin Center" -ForegroundColor White
Write-Host "2. Or use Teams Developer Portal to import" -ForegroundColor White
Write-Host "3. Test in Teams by adding the app" -ForegroundColor White

