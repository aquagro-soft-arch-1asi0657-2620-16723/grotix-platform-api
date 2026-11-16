#!/usr/bin/env bash
set -euo pipefail
cd /src
mode="${1:-}"
case "$mode" in
  list) command=(migrations list) ;;
  apply) command=(database update) ;;
  check-model) command=(migrations has-pending-model-changes) ;;
  *) echo 'Usage: migrator [list|apply|check-model]' >&2; exit 2 ;;
esac
if [[ -z "${ConnectionStrings__DefaultConnection:-}" || -z "${ConnectionStrings__TelemetryTimescale:-}" ]]; then
  echo 'Missing database connection strings' >&2; exit 1
fi
run_context() {
  local name="$1" project="$2" startup="$3"
  echo "=== ${name}: ${mode} ==="
  dotnet ef "${command[@]}" --context "$name" --project "$project" \
     --startup-project "$startup" --configuration Release --no-build
}
# Ordering matters: Profiles creates the original shared schema; IAM and Cultivation
# baseline migrations depend on that history. Avoid parallel migration execution.
run_context ProfilesDbContext src/Grotix.Persistence/Grotix.Persistence.csproj src/Profiles.Api/Profiles.Api.csproj
run_context IamDbContext src/Grotix.Persistence/Grotix.Persistence.csproj src/Profiles.Api/Profiles.Api.csproj
run_context CultivationAreaDbContext src/Grotix.Persistence/Grotix.Persistence.csproj src/CultivationArea.Api/CultivationArea.Api.csproj
run_context HardwareDeviceDbContext src/Grotix.Persistence.HardwareDevice/Grotix.Persistence.HardwareDevice.csproj src/HardwareDevice.Api/HardwareDevice.Api.csproj
run_context IrrigationCycleDbContext src/Grotix.Persistence.IrrigationCycle/Grotix.Persistence.IrrigationCycle.csproj src/IrrigationCycle.Api/IrrigationCycle.Api.csproj
run_context TelemetryDbContext src/Grotix.Persistence.Telemetry/Grotix.Persistence.Telemetry.csproj src/Telemetry.Api/Telemetry.Api.csproj
echo "Migration command '${mode}' finished."
