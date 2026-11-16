# Hardware MAC validation — test fixture alignment

## Cause of the failures

The strict EUI-48 MAC validator in `Microcontroller.NormalizeMacAddress` (six hexadecimal octets separated by colons) correctly rejects placeholder test addresses like `MAC` and `MAC_ADDRESS`. Those fixtures were used by unrelated maintenance, zone-binding, and acceptance scenarios; consequently seven tests failed before reaching their intended assertions. An eighth test compared the exact `ArgumentException.Message`, which now includes .NET's `ParamName` suffix.

## Changes made

- `src/HardwareDevice.Api.Tests/Acceptance/Steps/HardwareSteps.cs`: replace three invalid placeholder addresses with distinct, valid, locally administered EUI-48 test addresses.
- `src/HardwareDevice.Api.Tests/Integration/MaintenanceControllerTests.cs`: replace the invalid device address in the controller fixture.
- `src/HardwareDevice.Api.Tests/Unit/HardwareDomainAndServiceTests.cs`: replace two invalid device addresses and make the empty-address exception message assertion tolerate the standard .NET parameter suffix (`*`).
- **Unchanged**: the production MAC validator and the regression test `TC_U68_BindToZone_InvalidMacAddressFormat_ShouldThrowArgumentException`, which must continue to reject `00:1A:ZZ`.

## Local verification required

Run from the solution root with .NET 10 SDK:

```powershell
dotnet restore Grotix-backend.sln
dotnet build Grotix-backend.sln -c Release
dotnet test Grotix-backend.sln -c Release --no-build
```

Expected after these fixture corrections: 115/115 passing tests, if there are no other environment-dependent failures. This result **has not been executed or independently verified here** because the .NET SDK is not installed in this workspace. The user's prior log confirmed a successful full build, followed by 107/115 passing tests.

To check only the affected suite first:

```powershell
dotnet test src/HardwareDevice.Api.Tests/HardwareDevice.Api.Tests.csproj -c Release
```

Public deployment requires additional runtime, database, security, and HTTP smoke tests. Passing unit/acceptance tests alone does not establish production readiness.
