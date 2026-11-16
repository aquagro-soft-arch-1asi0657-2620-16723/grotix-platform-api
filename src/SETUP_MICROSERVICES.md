# Grotix — ejecución local por .NET CLI

Esta guía solo cubre ejecución local de desarrollo. Para producción ver
[`../docs/DEPLOYMENT_AZURE_VM.md`](../docs/DEPLOYMENT_AZURE_VM.md).

## Requisitos

- SDK .NET 10, MySQL 8, PostgreSQL con TimescaleDB; RabbitMQ si se
  demostrarán eventos asíncronos.
- Proporcionar `ConnectionStrings__DefaultConnection`,
  `ConnectionStrings__TelemetryTimescale` y `TokenSettings__Secret`
  como variables de entorno locales. Genera un JWT de mínimo 32 caracteres
  ASCII aleatorios. No reutilices los secretos que estuvieron en Git.
- Si hay browser local, `Cors__AllowedOrigins__0=http://localhost:5173`
  (adaptar al puerto real del frontend).
- Aplicar migraciones en forma controlada antes de iniciar servicios. En PowerShell,
  revisar la lista mediante `./migrate.ps1 -ListOnly` y, después de backup,
  ejecutar `./migrate.ps1 -Apply`.

## Ejecución por consola

```powershell
dotnet restore Grotix-backend.sln
dotnet build Grotix-backend.sln -c Release
dotnet run --project src/Profiles.Api --urls http://localhost:5101
dotnet run --project src/CultivationArea.Api --urls http://localhost:5102
dotnet run --project src/Telemetry.Api --urls http://localhost:5103
dotnet run --project src/HardwareDevice.Api --urls http://localhost:5104
dotnet run --project src/IrrigationCycle.Api --urls http://localhost:5105
dotnet run --project src/Gateway.Api --urls http://localhost:5100
```

Cada `dotnet run` se ejecuta en una consola diferente; alternativamente, usar
`start.ps1` en Windows con los perfiles definidos en `launchSettings.json`.
El gateway usa destinos `localhost` para desarrollo definidos en
`src/Gateway.Api/appsettings.Development.json`.

Comprobar `GET http://localhost:5100/live`,
`GET http://localhost:5100/ready/core` y
`GET http://localhost:5100/ready/telemetry`.
Para publicación cloud usar exclusivamente la configuración Compose principal.
