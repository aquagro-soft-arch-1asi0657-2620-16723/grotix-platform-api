# Estado de validación (reporte del equipo, 15–16 noviembre 2026)

## Ejecución real informada desde Windows 11/PowerShell

**Antes de generar el ZIP consolidado:**
- .NET 10 SDK 10.0.404, solution build Release completado con 11 warnings de nulabilidad.
- **115/115 tests aprobados**, sin fallos ni saltos; xUnit y SpecFlow en cinco proyectos de test.
- Docker Engine 29.8.2: `grotix-{gateway,profiles,cultivation,telemetry,hardware,irrigation}` y MySQL8/TimescaleDB UP.
- MySQL publicado solo en 127.0.0.1:3307; PG en 127.0.0.1:5433. Ambos healthy.
- Aplicadas 36 migraciones por seis contextos. Snapshot `CultivationAreaDbContextModelSnapshot.cs` actualizado, `dotnet ef migrations has-pending-model-changes` devuelve *No changes have been made to the model since the last migration.*
- GET `/live` + readiness (core/telemetry) HTTP 200; rutas health de hardware, telemetría, riego y documentación OpenAPI 200.
- POST `/api/v1/auth/sign-in` success=true y GET `/api/v1/profile/me` exitoso (admin de pruebas).
- Consultas GET a farms, crops, hardware devices, sensors/catalog, irrigation schedules: 200.
- CRUD `/api/v1/catalog/crops`: crear, consultar, actualizar, eliminar 204, verificar 404: exitoso.

## No está aún demostrado

- `docker compose.migrations.yml` (nuevo) aún no ejecutado fuera de esta entrega; migraciones Windows sí comprobadas.
- GitHub Actions CI/CD con **esta versión consolidada**, GHCR, DNS, HTTPS y API Azure real.
- Funcionalidades completas de cultivo, telemetría, mantenimiento e irrigación con datos y relaciones reales; no asumir que 115 tests mock cubren integración.
- RabbitMQ con eventos reales deshabilitado por defecto; Crop Analysis no incluido; autenticación y permisos completos de todos los roles todavía deben probarse.
- Recuperación de backups, límites de carga, rate limiting, monitoreo, pruebas de seguridad y operación productiva.

La preparación conserva el código y pruebas base; nunca rellenar evidencias académicas con capturas o URLs no verificadas.
