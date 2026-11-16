# Historial consolidado de Grotix — Preparación Azure VM

Base de este paquete: `Grotix-Platform-API-Backend-Snapshot-Fix.zip`, que integra las correcciones previas verificadas por el equipo.

## Código y pruebas

- Uniformización del backend y las pruebas a .NET 10; referencias `.csproj` corregidas.
- Actualización de tests Cultivation Area, Hardware, Telemetry e Irrigation para constructores que cambiaron.
- Validación estricta de MAC en el agregado Microcontroller; fixtures de prueba corregidos sin retirar validaciones.
- Dependencia MySqlConnector actualizada desde la versión vulnerable inicial.
- Snapshot de `CultivationAreaDbContext` actualizado según la entidad AnalysisReport existente; EF confirmó que no quedan cambios pendientes.
- Validaciones ejecutadas por el usuario: solution Release correcta; 115 pruebas aprobadas; 36 migraciones aplicadas; DB, gateway y microservicios iniciados; login/JWT y CRUD de cultivos funcionando.

## Seguridad y configuración

- Eliminadas credenciales fijas de ejemplos y arranque; bootstrap admin solo con variables explícitas; JWT requerido.
- CORS restringido; RabbitMQ desactivado hasta configurarlo; Swagger opt-in.
- No se incluyen `.env` ni secretos locales; se entregan tres plantillas y scripts para secretos aleatorios locales.
- .gitignore/.dockerignore ampliados para backups/dumps/snapshots temporales.

## Infraestructura nueva de este ZIP

- Puertos host de DB configurables, con valores por defecto 3307/5433 y loopback, replicando el entorno local validado por el usuario.
- `docker-compose.migrations.yml` + `deploy/migrator/`: migraciones en un contenedor SDK 10 + dotnet-ef 9, sin instalar el SDK en la VM. **Aún sin validación de ejecución con Docker**.
- `scripts/init-env.ps1`, `scripts/init-env.sh` para configuración nueva por descarga.
- `scripts/backup-local-databases.sh` para generación de dumps locales antes de actualizar.
- `scripts/deploy-release.sh` y workflow SSH de GHCR para despliegue manual sin migraciones automáticas.
- CI actualizado con validación estática y configuración de los tres archivos Compose.
- Documentación Azure VM, checklists de entrega y pasos de verificación real.

**No se realizaron operaciones sobre el repositorio GitHub ni Azure desde esta consolidación.** Las credenciales anteriormente expuestas deben rotarse antes de publicar. Una sola VM con DB locales no implica alta disponibilidad. Los flujos RabbitMQ/Crop Analysis requieren componentes y pruebas adicionales.
