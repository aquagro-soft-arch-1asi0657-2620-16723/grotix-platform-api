# Grotix Platform API

Backend de Grotix: gateway YARP y servicios de perfiles/IAM, áreas de cultivo, telemetría, dispositivos y ciclos de riego. Solución C# / ASP.NET Core 10, DDD por bounded contexts, persistencia MySQL y PostgreSQL/TimescaleDB y mensajería RabbitMQ opcional.

## Contenido

| Servicio | Proyecto | Público mediante Gateway |
| --- | --- | --- |
| Gateway | `src/Gateway.Api` | `/live`, `/api/v1/*`, `/docs/*/openapi.json` |
| Profiles / IAM | `src/Profiles.Api` | `/api/v1/auth/*`, `/api/v1/users/*`, `/api/v1/associations/*` |
| Cultivation Area | `src/CultivationArea.Api` | `/api/v1/farms/*`, `/api/v1/zones/*` |
| Telemetry | `src/Telemetry.Api` | `/api/v1/telemetry/*` |
| Hardware Device | `src/HardwareDevice.Api` | `/api/v1/hardware/*` |
| Irrigation Cycle | `src/IrrigationCycle.Api` | `/api/v1/irrigation/*` |

El módulo **Crop Analysis** NO está en este repositorio. La ruta `/api/v1/analysis/*` requiere desplegar su servicio Python por separado y configurar `CROP_ANALYSIS_URL`. No se declara completo su despliegue.

## Inicio con Docker

1. `cp .env.example .env`. Edita las credenciales, orígenes, bases de datos, y JWT con secretos nuevos. **No reutilices ninguna clave del repositorio original.** En Linux: `chmod 600 .env`.
2. Crea o configura MySQL 8 y PostgreSQL/TimescaleDB y aplica las migraciones EF Core existentes **antes** de desplegar APIs. Revisa la guía de Azure.
3. `docker compose config -q` para validar el archivo de composición. Si se requieren eventos con RabbitMQ, habilitar el broker opcional con `-f docker-compose.yml -f docker-compose.rabbitmq.yml` y `RABBITMQ_ENABLED=true`.
4. `docker compose up -d --build`.
5. `curl -fsS http://localhost:5100/live` y `curl -i http://localhost:5100/ready/core`.

Para publicar con HTTPS en una VM Azure, dirige el DNS `GROTIX_DOMAIN` a la IP pública, abre únicamente TCP 80/443 y ejecuta:

```bash
docker compose --profile public up -d --build
curl -fsS https://api.example.com/live
```

Solo el gateway se publica a través de Caddy; las APIs internas no exponen puertos externos. **Nunca publiques 3306, 5432, 5672 ni 15672 al Internet.**

## Desarrollo y pruebas

Requiere .NET 10 SDK, Docker Engine con Compose v2 y DB configuradas.

```bash
dotnet restore Grotix-backend.sln
dotnet build Grotix-backend.sln -c Release
dotnet test Grotix-backend.sln -c Release --no-build
python3 scripts/validate_deployment.py
bash scripts/smoke-test.sh http://localhost:5100
```

BDD: `src/*Api.Tests/Acceptance/Features/*.feature`. Los tests no reemplazan pruebas de integración contra bases de datos y RabbitMQ reales.

## Documentación

- [Despliegue en Azure VM, SSL y rollback](docs/DEPLOYMENT_AZURE_VM.md)
- [Pruebas, endpoints y evidencias para la evaluación](docs/TESTING_AND_EVIDENCE.md)
- [Auditoría de cambios y riesgos pendientes](docs/AUDIT_REPORT.md)
- [Configuración local con .NET CLI](src/SETUP_MICROSERVICES.md)

