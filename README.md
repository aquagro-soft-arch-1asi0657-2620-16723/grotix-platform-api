# Grotix Platform API

Backend C#/.NET 10, API Gateway YARP y microservicios DDD: **Profiles/IAM, Cultivation Area, Hardware Device, Telemetry e Irrigation Cycle**. Persistencia: MySQL 8 y PostgreSQL 16 + TimescaleDB. RabbitMQ opcional. Este repositorio contiene el código completo, las migraciones, las pruebas y el despliegue Docker/Azure VM.

> **Límite de alcance:** Crop Analysis (Python) NO forma parte de este repo; sus rutas requieren un despliegue externo. RabbitMQ está desactivado por defecto, por lo que los flujos asíncronos no están cubiertos por una demo exclusivamente HTTP.

## Inicio rápido: Windows con Docker Desktop (PowerShell)

Prerequisitos: Docker Desktop **iniciado** y PowerShell. No necesitas Visual Studio ni SDK .NET para levantar los contenedores; sí .NET 10 SDK para ejecutar las pruebas fuera de Docker.

```powershell
# Solo la primera vez. No sobreescribe un .env existente:
.\scripts\init-env.ps1 -Mode Local
# Si PowerShell bloquea scripts: Set-ExecutionPolicy -Scope Process Bypass
# Editar .env si necesitas cambiar los puertos locales (por defecto 3307, 5433)

docker compose -f docker-compose.yml -f docker-compose.databases.yml config --quiet
docker compose -f docker-compose.yml -f docker-compose.databases.yml up -d mysql postgres
docker compose -f docker-compose.yml -f docker-compose.databases.yml ps

# Primera instalación con BD vacía: comprobar y luego aplicar las 36 migraciones.
# El runner utiliza Docker + .NET 10 + EF Core 9, sin SDK instalado en Windows.
docker compose -f docker-compose.yml -f docker-compose.databases.yml -f docker-compose.migrations.yml run --rm --build migrator list
docker compose -f docker-compose.yml -f docker-compose.databases.yml -f docker-compose.migrations.yml run --rm migrator apply

docker compose -f docker-compose.yml -f docker-compose.databases.yml up -d --build
.\scripts\smoke-test.ps1 -OpenApi
```

> **Para quien ya ejecutó Grotix antes:** reutiliza su `.env` previo en privado si conservará los mismos volúmenes Docker; no regenere credenciales para usuarios de BD ya inicializados. Para nuevas instalaciones, genera secretos nuevos.

**Antes de `migrator apply` revisa el esquema y crea backup de bases con datos.** Las migraciones incluyen cambios de esquema potencialmente destructivos. En una base nueva vacía se pueden ejecutar secuencialmente en el orden configurado; no borres volúmenes para repetir el bootstrap. Los hosts internos son `mysql:3306`, `postgres:5432`; los puertos de Windows son `127.0.0.1:3307`, `127.0.0.1:5433` (configurables).

El script `init-env.ps1` genera credenciales **únicas y privadas** en `.env`. Para probar el administrador inicial, consulta `BOOTSTRAP_ADMIN_EMAIL/PASSWORD` solo en tu equipo; no publiques secretos ni tokens. Después del primer login elimina ambas variables y recrea el servicio `profiles`.

## Inicio rápido: Linux / VM Azure

Si es una VM única con bases autocontenidas, usa el flujo de [Azure VM de principio a fin](docs/DEPLOYMENT_AZURE_VM.md). Resumen:

```bash
bash scripts/init-env.sh azure-vm
# editar .env: dominio real, origen frontend, email admin, etc.
# configurar DNS A -> IP VM y abrir 80/443 en Azure NSG
# preparar MySQL/TimescaleDB; aplicar migraciones con el runner
# publicar a través de Caddy HTTPS con `--profile public`
```

`docker-compose.yml` sirve para APIs con DB externa; `docker-compose.databases.yml` agrega DB en Docker; `docker-compose.migrations.yml` incluye el runner ef Core de una sola ejecución. **No ejecutes el runner contra bases ajenas sin backup y autorización.**

## Servicios y endpoints

| Servicio | Proyecto | Gateway |
| --- | --- | --- |
| Gateway | `src/Gateway.Api` | `/live`, `/ready/core`, `/ready/telemetry` |
| Profiles/IAM | `src/Profiles.Api` | `/api/v1/auth/*`, `/api/v1/profile/me` |
| Cultivation Area | `src/CultivationArea.Api` | `/api/v1/farms`, `/api/v1/catalog/crops` |
| Hardware Device | `src/HardwareDevice.Api` | `/api/v1/hardware/*` |
| Irrigation Cycle | `src/IrrigationCycle.Api` | `/api/v1/irrigation/*` |
| Telemetry | `src/Telemetry.Api` | `/api/v1/telemetry/*` |

Gateway local: `http://localhost:5100`. OpenAPI JSON (si `SWAGGER_ENABLED=true`): `/docs/profiles/openapi.json`, `/docs/irrigation/openapi.json`. Los tests de salud **no** sustituyen un test REST autenticado.

## Pruebas y compilación

```bash
dotnet restore Grotix-backend.sln
dotnet build Grotix-backend.sln -c Release
dotnet test Grotix-backend.sln -c Release --no-build
python3 scripts/validate_deployment.py
```

**Validación hecha por el equipo en Windows antes de este empaquetado:** 115/115 tests aprobados, compilación completa, migraciones MySQL/TimescaleDB, snapshot EF Core corregido, 6 contenedores operativos, JWT, 5 GET y CRUD de cultivos. **No son evidencia de que Azure esté desplegado.** La nueva envoltura de automatización Linux y los workflows necesitan ejecución final en GitHub Actions / Azure.

## CI/CD, versiones y seguridad

- `.github/workflows/ci.yml`: compila, prueba y revisa archivos de despliegue.
- `.github/workflows/publish-images.yml`: publica seis imágenes GHCR mediante un tag SemVer `vX.Y.Z`.
- `.github/workflows/deploy-azure-vm.yml`: despliega manualmente una imagen ya publicada, por SSH y environment `production`.
- `scripts/deploy-release.sh`: despliega imágenes publicadas sin lanzar migraciones automáticamente, respeta `GROTIX_DATABASE_MODE=vm|external`.
- `.env.local.example`, `.env.azure-vm.example`, `.env.example`: plantillas **sin** secretos reales. `.env` se ignora en Git.

No publiques `.env`, dumps, SSH keys, contraseñas, JWT ni capturas que los contengan. Cambia las credenciales de los ZIP anteriores y rota cualquiera que hubiera sido compartida. Cambia `GROTIX_DOMAIN`, `CORS_ALLOWED_ORIGIN`, admin bootstrap y SSL antes del despliegue público. Las APIs solo están accesibles públicamente mediante Caddy (443); no expongas MySQL/PostgreSQL/RabbitMQ.

## Guías

- [Azure VM: desde cero, migraciones, HTTPS y GitHub Actions](docs/DEPLOYMENT_AZURE_VM.md)
- [Checklist de verificación antes de publicar](docs/RELEASE_CHECKLIST.md)
- [Validación local comprobada y riesgos restantes](docs/VALIDATION_STATUS.md)
- [Auditoría técnica de cambios](docs/AUDIT_REPORT.md)
- [Suites y evidencia académica](docs/TESTING_AND_EVIDENCE.md)
