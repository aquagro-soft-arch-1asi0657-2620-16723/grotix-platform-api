# Local validation fixes — Grotix backend

## Origin of these fixes

Based on the first real Windows .NET SDK 10.0.404 build log provided by the project team:
- `dotnet restore Grotix-backend.sln`: successful, with warnings.
- `dotnet build Grotix-backend.sln -c Release`: failed with 20 errors in four test projects.
- `Profiles.Api.Tests`: 24 tests passed in the partial run.
- `dotnet run --project src/Gateway.Api/Gateway.Api.csproj`: failed because `TokenSettings:Secret` was not configured.

## Corrections applied

- CultivationArea tests: updated `Zone` calls to pass `Name` and used the current order for `CreateZoneCommand`, `UpdateZoneCommand`, and `PatchZoneRequest`.
- HardwareDevice tests: supplied `IStaffExistenceService` before `IHardwareDeviceUnitOfWork` to `MaintenanceService`, and configured an existing staff member in tests that create technical maintenance.
- Telemetry test: supplied an `IDeviceHeartbeatPublisher` mock.
- IrrigationCycle test: supplied an `IIrrigationStartedPublisher` mock.
- Removed unnecessary `Microsoft.Extensions.Configuration.UserSecrets` direct dependencies flagged by NU1510.
- Pinned `MySqlConnector` 2.6.2 wherever the Pomelo provider is referenced, addressing GHSA-473q-m89c-ghf8 (fixed in 2.6.1). Restore against NuGet is required to validate transitive resolution.
- JWT bootstrapping now accepts `JWT_SECRET` from `.env` when `TokenSettings:Secret` is absent. A secret of at least 32 ASCII characters is still required.

## Commands (run in the project root)

```powershell
dotnet restore Grotix-backend.sln
dotnet build Grotix-backend.sln -c Release
dotnet test Grotix-backend.sln -c Release --no-build
```

For local development, use a random secret **in the current terminal only**:

```powershell
$bytes = New-Object byte[] 48
[System.Security.Cryptography.RandomNumberGenerator]::Fill($bytes)
$env:TokenSettings__Secret = [Convert]::ToBase64String($bytes)
dotnet run --project src/Gateway.Api/Gateway.Api.csproj
```

Check `http://localhost:<printed-port>/live` (the local port depends on launch settings). The gateway running does not prove the other services or databases are available.

## Verification status

These fixes are statically checked against the interfaces and constructors bundled in the ZIP; **the full .NET build, test suite, database migrations, and Docker deployment have not been run by the assistant**. Run the commands above and address any remaining failing assertions before a public deployment.

## References

- MySqlConnector advisory: https://github.com/advisories/GHSA-473q-m89c-ghf8
- NuGet 2.6.2: https://www.nuget.org/packages/MySqlConnector/2.6.2

## Second local validation — MAC address domain invariant (15 Nov 2026)

The team then ran .NET SDK 10.0.404 on Windows and reported:

- `dotnet restore Grotix-backend.sln`: succeeded.
- `dotnet build Grotix-backend.sln -c Release`: succeeded with 11 nullable-reference warnings.
- `dotnet test Grotix-backend.sln -c Release --no-build`: 114 passed, 1 failed (115 total).
- Failing test: `HardwareSprint3UnitTests.TC_U68_BindToZone_InvalidMacAddressFormat_ShouldThrowArgumentException`.

### Root cause and correction

The domain entity `Microcontroller` checked that MAC addresses were nonempty but did not check hexadecimal octets or length. Thus `00:1A:ZZ` was incorrectly accepted.

Corrected `Grotix/HardwareDevice/Domain/Model/Aggregates/Microcontroller.cs`:
- Validate that the supplied MAC address has exactly six 2-character hexadecimal groups separated by colons, e.g. `00:1A:2B:3C:4D:5E`.
- Uppercase and trim valid input as before, preserving normal storage semantics.
- Throw `ArgumentException` for malformed MAC addresses when creating or updating devices.
- Preserve the original test unchanged to verify the behavior.

### Retest on Windows

```powershell
dotnet restore Grotix-backend.sln
dotnet build Grotix-backend.sln -c Release
dotnet test Grotix-backend.sln -c Release --no-build
```

Optionally run only the originally failing test first:

```powershell
dotnet test src/HardwareDevice.Api.Tests/HardwareDevice.Api.Tests.csproj -c Release --filter "FullyQualifiedName~TC_U68_BindToZone_InvalidMacAddressFormat_ShouldThrowArgumentException"
```

**Status:** static source inspection and ZIP integrity checks completed by the assistant; the patched .NET build and full test suite must still be executed in the user's Windows environment. The other 11 warnings do not prevent building but merit future cleanup. Database connectivity, integration with external systems, and public deployment remain unverified.
