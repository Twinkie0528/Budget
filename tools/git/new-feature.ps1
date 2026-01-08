<#
.SYNOPSIS
    Creates a new feature branch following naming conventions.

.DESCRIPTION
    Creates a feature branch from the latest develop branch with proper naming.

.PARAMETER Name
    The feature name (will be slugified).

.PARAMETER Type
    The branch type: feature, bugfix, hotfix. Default: feature

.EXAMPLE
    .\new-feature.ps1 -Name "add user export" -Type feature
    # Creates: feature/add-user-export

.EXAMPLE
    .\new-feature.ps1 -Name "fix import validation" -Type bugfix
    # Creates: bugfix/fix-import-validation
#>

param(
    [Parameter(Mandatory = $true)]
    [string]$Name,
    
    [ValidateSet("feature", "bugfix", "hotfix")]
    [string]$Type = "feature"
)

$ErrorActionPreference = "Stop"

# Slugify the name
$slug = $Name.ToLower() -replace '[^a-z0-9]+', '-' -replace '^-|-$', ''
$branchName = "$Type/$slug"

Write-Host "Creating branch: $branchName" -ForegroundColor Cyan

# Fetch latest
Write-Host "Fetching latest changes..." -ForegroundColor Yellow
git fetch origin

# Determine base branch
$baseBranch = if ($Type -eq "hotfix") { "main" } else { "develop" }

# Check if base branch exists
$remoteBranch = git branch -r --list "origin/$baseBranch"
if (-not $remoteBranch) {
    $baseBranch = "main"
}

Write-Host "Base branch: $baseBranch" -ForegroundColor Yellow

# Create and checkout branch
git checkout -b $branchName "origin/$baseBranch"

if ($LASTEXITCODE -eq 0) {
    Write-Host ""
    Write-Host "Branch created successfully!" -ForegroundColor Green
    Write-Host ""
    Write-Host "Next steps:" -ForegroundColor Cyan
    Write-Host "  1. Make your changes" -ForegroundColor White
    Write-Host "  2. Commit: git commit -m 'feat: $Name'" -ForegroundColor White
    Write-Host "  3. Push: git push -u origin $branchName" -ForegroundColor White
    Write-Host "  4. Create PR on GitHub" -ForegroundColor White
} else {
    Write-Host "Failed to create branch" -ForegroundColor Red
    exit 1
}

