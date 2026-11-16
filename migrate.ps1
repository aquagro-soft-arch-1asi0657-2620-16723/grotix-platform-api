# Grotix: Database migration runner (no embedded credentials).
# For both MySQL and TimescaleDB set the connection strings in the process environment.
# Required: dotnet tool install --global dotnet-ef --version 9.*
# Usage: .\migrate.ps1 -ListOnly  |  .\migrate.ps1 -Apply
[CmdletBinding()]
param(
    [switch]$ListOnly,
    [switch]$Apply
)
$ErrorActionPreference = 'Stop'

if ($ListOnly.IsPresent -eq $Apply.IsPresent) {
    throw 'Select exactly one mode: -ListOnly or -Apply. The script never migrates by default.'
}
if ([string]::IsNullOrWhiteSpace($env:ConnectionStrings__DefaultConnection)) {
    throw 'Provide ConnectionStrings__DefaultConnection in your shell environment.'
}
if ([string]::IsNullOrWhiteSpace($env:ConnectionStrings__TelemetryTimescale)) {
    throw 'Provide ConnectionStrings__TelemetryTimescale in your shell environment.'
}

$contexts = @(
    @{ Context='ProfilesDbContext'; Project='src/Grotix.Persistence/Grotix.Persistence.csproj'; Startup='src/Profiles.Api/Profiles.Api.csproj' },
    @{ Context='IamDbContext'; Project='src/Grotix.Persistence/Grotix.Persistence.csproj'; Startup='src/Profiles.Api/Profiles.Api.csproj' },
    @{ Context='CultivationAreaDbContext'; Project='src/Grotix.Persistence/Grotix.Persistence.csproj'; Startup='src/CultivationArea.Api/CultivationArea.Api.csproj' },
    @{ Context='HardwareDeviceDbContext'; Project='src/Grotix.Persistence.HardwareDevice/Grotix.Persistence.HardwareDevice.csproj'; Startup='src/HardwareDevice.Api/HardwareDevice.Api.csproj' },
    @{ Context='IrrigationCycleDbContext'; Project='src/Grotix.Persistence.IrrigationCycle/Grotix.Persistence.IrrigationCycle.csproj'; Startup='src/IrrigationCycle.Api/IrrigationCycle.Api.csproj' },
    @{ Context='TelemetryDbContext'; Project='src/Grotix.Persistence.Telemetry/Grotix.Persistence.Telemetry.csproj'; Startup='src/Telemetry.Api/Telemetry.Api.csproj' }
)
$command = if ($ListOnly) { 'migrations' } else { 'database' }
$action = if ($ListOnly) { 'list' } else { 'update' }
foreach ($item in $contexts) {
    Write-Host "[$($item.Context)] $command $action" -ForegroundColor Cyan
    $args = @('ef', $command, $action,
        '--context', $item.Context,
        '--project', $item.Project,
        '--startup-project', $item.Startup)
    & dotnet @args
    if ($LASTEXITCODE -ne 0) {
        throw "EF command failed for $($item.Context), exit code $LASTEXITCODE"
    }
}
Write-Host 'Commands finished. Inspect actual database schemas and logs.' -ForegroundColor Green
