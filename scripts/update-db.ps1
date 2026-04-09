param(
    [string]$Environment = 'Development'
)

$ErrorActionPreference = 'Stop'

$repoRoot = Split-Path -Parent $PSScriptRoot
$project = '.\Shared.Infrastructure\Shared.Infrastructure.csproj'

Push-Location $repoRoot
$previousEnvironment = $env:ASPNETCORE_ENVIRONMENT
try {
    $env:ASPNETCORE_ENVIRONMENT = $Environment

    Write-Host "Applying AppDbContext migrations ($Environment)..."
    dotnet ef database update `
        --project $project `
        --startup-project $project `
        --context AppDbContext

    if ($LASTEXITCODE -ne 0) {
        exit $LASTEXITCODE
    }

    Write-Host "Applying AuthDbContext migrations ($Environment)..."
    dotnet ef database update `
        --project $project `
        --startup-project $project `
        --context AuthDbContext

    if ($LASTEXITCODE -ne 0) {
        exit $LASTEXITCODE
    }

    Write-Host "Database migrations applied successfully."
}
finally {
    $env:ASPNETCORE_ENVIRONMENT = $previousEnvironment
    Pop-Location
}
