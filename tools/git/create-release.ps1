<#
.SYNOPSIS
    Creates a new release tag.

.DESCRIPTION
    Creates a semantic version tag and pushes it to trigger the release workflow.

.PARAMETER Version
    The version number (without 'v' prefix). E.g., "1.0.0"

.PARAMETER Message
    Optional release message/description.

.EXAMPLE
    .\create-release.ps1 -Version "1.0.0" -Message "Initial release"

.EXAMPLE
    .\create-release.ps1 -Version "1.1.0"
#>

param(
    [Parameter(Mandatory = $true)]
    [ValidatePattern('^\d+\.\d+\.\d+(-[a-zA-Z0-9]+)?$')]
    [string]$Version,
    
    [string]$Message = ""
)

$ErrorActionPreference = "Stop"

$tagName = "v$Version"

Write-Host "Creating release: $tagName" -ForegroundColor Cyan

# Check we're on main branch
$currentBranch = git branch --show-current
if ($currentBranch -ne "main") {
    Write-Host "Warning: You're not on the main branch (current: $currentBranch)" -ForegroundColor Yellow
    $confirm = Read-Host "Continue anyway? (y/N)"
    if ($confirm -ne "y") {
        Write-Host "Aborted" -ForegroundColor Red
        exit 1
    }
}

# Check for uncommitted changes
$status = git status --porcelain
if ($status) {
    Write-Host "Error: You have uncommitted changes. Please commit or stash them first." -ForegroundColor Red
    exit 1
}

# Fetch and check if tag exists
git fetch --tags
$existingTag = git tag -l $tagName
if ($existingTag) {
    Write-Host "Error: Tag $tagName already exists" -ForegroundColor Red
    exit 1
}

# Create tag
if ($Message) {
    git tag -a $tagName -m "$Message"
} else {
    git tag -a $tagName -m "Release $Version"
}

Write-Host "Tag created locally: $tagName" -ForegroundColor Green

# Push tag
Write-Host "Pushing tag to origin..." -ForegroundColor Yellow
git push origin $tagName

if ($LASTEXITCODE -eq 0) {
    Write-Host ""
    Write-Host "Release $tagName created and pushed!" -ForegroundColor Green
    Write-Host ""
    Write-Host "The CD workflow will now:" -ForegroundColor Cyan
    Write-Host "  1. Build the application" -ForegroundColor White
    Write-Host "  2. Deploy to production" -ForegroundColor White
    Write-Host "  3. Package Teams app" -ForegroundColor White
    Write-Host ""
    Write-Host "Monitor the workflow at: https://github.com/YOUR_ORG/budget-platform/actions" -ForegroundColor Yellow
} else {
    Write-Host "Failed to push tag. Rolling back..." -ForegroundColor Red
    git tag -d $tagName
    exit 1
}

