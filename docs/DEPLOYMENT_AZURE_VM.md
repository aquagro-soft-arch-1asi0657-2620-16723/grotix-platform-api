# Grotix — Despliegue público en Azure VM (HTTPS)

Guía operativa para el backend .NET 10. No incluye la creación automática de recursos ni credenciales. En el informe final registrar la región, recursos, tamaño de VM, NSG, costos, DNS, branch, commit y URL públicos **reales**.

## Arquitectura propuesta

```mermaid
flowchart TD
  User[App web/móvil] --> HTTPS[HTTPS 443 / Azure VM + Caddy]
  HTTPS --> Gateway[YARP Gateway :8080]
  Gateway --> Profiles[Profiles / IAM]
  Gateway --> Cultivation[CultivationArea]
  Gateway --> Telemetry[Telemetry]
  Gateway --> Hardware[HardwareDevice]
  Gateway --> Irrigation[IrrigationCycle]
  Profiles --> MySQL[(Azure MySQL Flexible Server)]
  Cultivation --> MySQL
  Hardware --> MySQL
  Irrigation --> MySQL
  Telemetry --> Pg[(PostgreSQL + TimescaleDB)]
  Telemetry --> MySQL
  Profiles -. opcional .-> MQ[(RabbitMQ)]
  Irrigation -. opcional .-> MQ
  Gateway -. módulo externo .-> Crop[Crop Analysis API (otro repo)]
```

> **TimescaleDB** requiere la extensión disponible y habilitada en el PostgreSQL seleccionado. No asumir que cualquier instancia administrada Azure la admite. Comprobar previamente la compatibilidad; en su defecto usar PostgreSQL/TimescaleDB autogestionado en host seguro o ajustar los scripts y consultas según el motor usado.

## 1. Preparación de infraestructura

1. VM Linux Ubuntu 24.04 LTS, IP pública estática, DNS A `api.tudominio.com`. Configurar SSH por llave; restringir origen del puerto 22 a IPs administrativas.
2. Instalar Docker Engine y plugin Compose v2 según instrucciones oficiales. Habilitar solo TCP 80 y 443 en NSG y firewall; 22 solo desde red de administración. Evitar exponer servicios internos.
3. Proveer MySQL 8 compatible con Pomelo EF Core 9 y PostgreSQL + extensión TimescaleDB accesibles desde la red de la VM. Configurar firewall de BD solo desde el origen de backend y conexiones TLS. Proveer copias de seguridad y usuario de BD con mínimos privilegios (separar rol de migraciones).
4. Registrar valores de conexión como secretos del entorno (`.env` local fuera del versionado y con `chmod 600`). El JWT debe ser aleatorio y único para todo el cluster, con 32+ caracteres ASCII, y **no** puede ser el de ejemplo.
5. Para ingesta/eventos asíncronos (telemetría→riego), provisionar RabbitMQ por red privada; `RABBITMQ_ENABLED=true`, credenciales propias, y para un broker externo establecer `RABBITMQ_URI=amqps://...`. Con `false` hay funcionalidades orientadas a eventos que no operan, aunque el HTTP funcione. Para una demo en **una sola VM**, opcionalmente usar `docker-compose.rabbitmq.yml` junto con el archivo principal, sin publicar AMQP al exterior: `docker compose -f docker-compose.yml -f docker-compose.rabbitmq.yml --profile public up -d --build`. En el `.env` establecer `RABBITMQ_HOST=rabbitmq`, `RABBITMQ_ENABLED=true` y una clave nueva.

## 2. Preparar proyecto

```bash
git clone https://github.com/Aguagro/grotix-platform-api.git
cd grotix-platform-api
cp .env.example .env
chmod 600 .env
# editar .env con editor local, NO adjuntar capturas de su contenido
openssl rand -hex 32   # usar resultado aleatorio NUEVO en JWT_SECRET
```

Verificar que **no** se haya dejado `REPLACE_ME`, que `MYSQL_HOST`, `POSTGRES_HOST`, TLS y `CORS_ALLOWED_ORIGIN` son correctos y que `GROTIX_DOMAIN` corresponde al DNS. Si se usa el modo sin dominio para pruebas locales, no activar perfil `public`.

## 3. Migraciones EF (ANTES de publicar)

Con .NET 10 SDK y `dotnet-ef` compatible con EF Core 9, conectar temporalmente por red segura a las BD y usar las migraciones ya incluidas. Se incluye `migrate.ps1`, con modo revisión (`-ListOnly`) y modo aplicación explícito (`-Apply`); validar rutas y cambios de esquema antes del uso. Tomar backup previo, revisar migraciones pendientes y probar restauración. **No ejecutar migraciones en paralelo desde seis contenedores** ni confiar en que el arranque realiza el esquema automáticamente.

En Windows / PowerShell, para los contextos que ya usa el proyecto, revisar primero:

```powershell
Get-Content .\migrate.ps1
./migrate.ps1 -ListOnly
# Solo tras inspeccionar el estado y confirmar el backup:
./migrate.ps1 -Apply
```

Los factories de diseño requieren `ConnectionStrings__DefaultConnection` o `ConnectionStrings__TelemetryTimescale` según corresponda. No usar credenciales root para el proceso web. Verificar con `dotnet ef migrations list ...` que los contextos registran todas las migraciones esperadas.

## 4. Publicación manual

```bash
docker compose config -q
docker compose --profile public up -d --build
docker compose ps
docker compose logs --tail=100 gateway profiles cultivation telemetry hardware irrigation
curl -fsS https://api.tudominio.com/live
curl -i https://api.tudominio.com/ready/core
curl -i https://api.tudominio.com/ready/telemetry
```

`/live` devuelve HTTP 200 si el gateway sirve HTTP. `/ready/core` y `/ready/telemetry` comprueban BDs desde Profiles y pueden devolver 503 si faltan migraciones, credenciales o red. No tomar `/live` como prueba de integración completa.

Para inspeccionar OpenAPI (solo cuando `SWAGGER_ENABLED=true`):

```bash
curl -i https://api.tudominio.com/docs/profiles/openapi.json
curl -i https://api.tudominio.com/docs/irrigation/openapi.json
```

Configurar `BOOTSTRAP_ADMIN_EMAIL` y `BOOTSTRAP_ADMIN_PASSWORD` **solo** al crear el administrador inicial; los campos vacíos impiden que se fabrique un usuario con credenciales conocidas. Eliminar la contraseña del `.env` después del bootstrap y recrear perfiles (o reiniciar) para que deje de aparecer en inspect del contenedor.

## 5. CI/CD y versiones

- `.github/workflows/ci.yml`: restore + build + pruebas .NET 10 + validación de configuración; se dispara en PR y push a `main/develop`.
- `.github/workflows/publish-images.yml`: tras un tag semver `vX.Y.Z` construye las seis imágenes y las publica en GHCR usando GITHUB_TOKEN.
- `.github/workflows/deploy-azure-vm.yml`: despliegue manual por SSH con tags (requiere GitHub Environment `production`, secrets `AZURE_VM_HOST`, `AZURE_VM_USER`, `AZURE_VM_SSH_KEY`, `AZURE_VM_KNOWN_HOSTS`, `AZURE_VM_REPO_PATH`). Requiere paquetes GHCR públicos o login de GHCR preparado en el host. Habilitar revisores requeridos para el environment `production`.

Para publicar desde imágenes del registry en la VM, configurar en `.env`:

```
IMAGE_PREFIX=ghcr.io/aguagro/grotix-
IMAGE_TAG=v1.0.0
```

Ejecutar `docker compose --profile public pull && docker compose --profile public up -d --no-build`. La etiqueta `v1.0.0` es solo un ejemplo: usar el tag **real** que publique el grupo. Preferir tags inmutables/digests en ambientes más estrictos.

## 6. Rollback y operaciones

- Conservar el tag anterior; revertir `IMAGE_TAG` al tag anterior conocido y volver a ejecutar `pull` + `up -d --no-build`.
- Si hubo migraciones destructivas de esquema, el rollback de imágenes no es suficiente: usar plan de migraciones compatibles y backup probado.
- Registrar logs y eventos sin JWT, passwords ni cadenas de conexión. No subir `.env`, claves ni capturas de credenciales.
- Para módulos externos ausentes, usar `CROP_ANALYSIS_URL` hacia un host funcional seguro y alcanzable; hasta entonces `/api/v1/analysis/*` no está listo.
- Un único servidor VM implica riesgo de disponibilidad. Para operación real evaluar Azure Container Apps/AKS, secretos gestionados, monitoreo y alta disponibilidad.

## Evidencia recomendada para entrega final

Capturas reales: VM + NSG con reglas 80/443, DNS, contenedores en ejecución, `/live` + readiness, Swagger JSON, un request Postman autenticado y respuesta, logs de pipeline exitoso, commits/PR y un pequeño video de demostración. No presentar estas capturas como realizadas antes de ejecutar el despliegue.
