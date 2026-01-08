<#
.SYNOPSIS
    Seeds the Budget Platform database with initial data.

.DESCRIPTION
    This script inserts sample dimension values and field schemas.
    Make sure the database is migrated before running this script.

.EXAMPLE
    .\seed.ps1
#>

$ErrorActionPreference = "Stop"

Write-Host "Seeding database with initial data..." -ForegroundColor Cyan

$connectionString = "Host=localhost;Port=5432;Database=budget_platform;Username=budget_user;Password=budget_pass"

# SQL seed script
$seedSql = @"
-- Insert Channel dimension values
INSERT INTO dimension_values (id, enum_key, code, name, description, sort_order, is_active, created_at, created_by)
VALUES 
    (gen_random_uuid(), 'channel', 'DIGITAL', 'Digital', 'Digital marketing channels', 1, true, NOW(), 'seed'),
    (gen_random_uuid(), 'channel', 'RETAIL', 'Retail', 'Retail channels', 2, true, NOW(), 'seed'),
    (gen_random_uuid(), 'channel', 'WHOLESALE', 'Wholesale', 'Wholesale channels', 3, true, NOW(), 'seed'),
    (gen_random_uuid(), 'channel', 'ECOMMERCE', 'E-Commerce', 'E-commerce platforms', 4, true, NOW(), 'seed')
ON CONFLICT (enum_key, code) DO NOTHING;

-- Insert Owner dimension values
INSERT INTO dimension_values (id, enum_key, code, name, description, sort_order, is_active, created_at, created_by)
VALUES 
    (gen_random_uuid(), 'owner', 'MARKETING', 'Marketing Team', 'Marketing department', 1, true, NOW(), 'seed'),
    (gen_random_uuid(), 'owner', 'SALES', 'Sales Team', 'Sales department', 2, true, NOW(), 'seed'),
    (gen_random_uuid(), 'owner', 'OPERATIONS', 'Operations Team', 'Operations department', 3, true, NOW(), 'seed'),
    (gen_random_uuid(), 'owner', 'FINANCE', 'Finance Team', 'Finance department', 4, true, NOW(), 'seed')
ON CONFLICT (enum_key, code) DO NOTHING;

-- Insert Frequency dimension values
INSERT INTO dimension_values (id, enum_key, code, name, description, sort_order, is_active, created_at, created_by)
VALUES 
    (gen_random_uuid(), 'frequency', 'MONTHLY', 'Monthly', 'Monthly recurring', 1, true, NOW(), 'seed'),
    (gen_random_uuid(), 'frequency', 'QUARTERLY', 'Quarterly', 'Quarterly recurring', 2, true, NOW(), 'seed'),
    (gen_random_uuid(), 'frequency', 'ANNUAL', 'Annual', 'Annual/yearly', 3, true, NOW(), 'seed'),
    (gen_random_uuid(), 'frequency', 'ONETIME', 'One-Time', 'One-time expense', 4, true, NOW(), 'seed')
ON CONFLICT (enum_key, code) DO NOTHING;

-- Insert Vendor dimension values
INSERT INTO dimension_values (id, enum_key, code, name, description, sort_order, is_active, created_at, created_by)
VALUES 
    (gen_random_uuid(), 'vendor', 'INTERNAL', 'Internal', 'Internal resources', 1, true, NOW(), 'seed'),
    (gen_random_uuid(), 'vendor', 'EXTERNAL', 'External Vendor', 'External vendors', 2, true, NOW(), 'seed'),
    (gen_random_uuid(), 'vendor', 'AGENCY', 'Agency', 'Marketing/Creative agencies', 3, true, NOW(), 'seed'),
    (gen_random_uuid(), 'vendor', 'CONTRACTOR', 'Contractor', 'Independent contractors', 4, true, NOW(), 'seed')
ON CONFLICT (enum_key, code) DO NOTHING;

-- Insert sample field schemas for extensibility
INSERT INTO field_schemas (id, field_key, label, field_type, is_required, applies_to, sort_order, is_active, created_at, created_by)
VALUES 
    (gen_random_uuid(), 'campaign_name', 'Campaign Name', 0, false, 0, 1, true, NOW(), 'seed'),
    (gen_random_uuid(), 'start_date', 'Start Date', 3, false, 0, 2, true, NOW(), 'seed'),
    (gen_random_uuid(), 'end_date', 'End Date', 3, false, 0, 3, true, NOW(), 'seed'),
    (gen_random_uuid(), 'approval_status', 'Approval Status', 6, false, 0, 4, true, NOW(), 'seed'),
    (gen_random_uuid(), 'notes', 'Additional Notes', 8, false, 1, 5, true, NOW(), 'seed')
ON CONFLICT (field_key) DO NOTHING;

SELECT 'Seed completed!' as status;
"@

# Check if psql is available, otherwise use dotnet tool
try {
    # Try using psql directly
    $env:PGPASSWORD = "budget_pass"
    $seedSql | psql -h localhost -p 5432 -U budget_user -d budget_platform
    Write-Host "Database seeded successfully using psql!" -ForegroundColor Green
}
catch {
    Write-Host "psql not found. Please run the seed SQL manually or install PostgreSQL client tools." -ForegroundColor Yellow
    Write-Host "SQL to execute:" -ForegroundColor Cyan
    Write-Host $seedSql
}

