# Grotix Platform API — Empezar aquí

**Este ZIP contiene el código fuente completo consolidado**, no solo los parches. Está preparado para que lo descargue un integrante, configure sus propias credenciales y lo ejecute con Docker en Windows o Ubuntu/Azure VM.

## Windows local (PowerShell, con Docker Desktop iniciado)

```powershell
.\scripts\init-env.ps1 -Mode Local
docker compose -f docker-compose.yml -f docker-compose.databases.yml up -d mysql postgres
docker compose -f docker-compose.yml -f docker-compose.databases.yml -f docker-compose.migrations.yml run --rm --build migrator list
# Solo sobre bases vacías y tras revisar los cambios:
docker compose -f docker-compose.yml -f docker-compose.databases.yml -f docker-compose.migrations.yml run --rm migrator apply
docker compose -f docker-compose.yml -f docker-compose.databases.yml up -d --build
.\scripts\smoke-test.ps1 -OpenApi
```

Si PowerShell impide ejecutar `.ps1`: `Set-ExecutionPolicy -Scope Process -ExecutionPolicy Bypass` aplica solo a la terminal actual. Se recomienda inspeccionar cualquier script antes de ejecutarlo.

**Si ya usabas Grotix con los volúmenes `grotix_mysql_data` y `grotix_postgres_data`:** conserva y copia manualmente tu `.env` anterior a esta nueva carpeta (en privado), en lugar de generar otro. Las contraseñas de MySQL/PostgreSQL se fijaron al crear esos volúmenes: cambiarlas solo en un `.env` nuevo no cambia los usuarios existentes y provocará fallos de conexión. Para empezar desde cero, usa una instalación/volúmenes nuevos, sin borrar los datos anteriores salvo decisión explícita.

La base del equipo ya fue validada con 115/115 pruebas, migraciones, autenticación y CRUD de cultivos; el **nuevo migrador Docker** y los nuevos scripts deben validarse en la máquina donde se ejecuten. Para compilar o probar con .NET 10 SDK:

```powershell
dotnet build Grotix-backend.sln -c Release
dotnet test Grotix-backend.sln -c Release --no-build
```

## Ubuntu / Azure VM

```bash
bash scripts/init-env.sh azure-vm
nano .env   # configurar dominio de la VM, origen del frontend y email del admin
chmod 600 .env
```

A continuación seguir **[docs/DEPLOYMENT_AZURE_VM.md](docs/DEPLOYMENT_AZURE_VM.md)**: crear VM/IP/DNS/NSG, ejecutar bases, migraciones, servicios con `--profile public` y verificar HTTPS público. La VM necesita Docker Engine y Compose v2; **no** necesita SDK .NET instalado fuera de Docker.

## Antes de subir a GitHub

- Descomprime y versiona los archivos fuente, **NO el ZIP**.
- Nunca subas `.env`, claves SSH, `backups/`, `bin/`, `obj/` ni archivos `.bak`.
- Revisa los cambios en una rama `feature/backend-deployment`, abre PR y ejecuta GitHub Actions.
- `GROTIX_DOMAIN=api.example.com` es un placeholder, no una URL funcional. Necesitas el dominio real y su DNS para HTTPS.
- Crop Analysis **no está incluido**; RabbitMQ viene deshabilitado. No afirmar que esos módulos están desplegados.

**Estado:** código consolidado y preparado para instalar y desplegar, pero **Azure real no ha sido provisionado ni validado aún**. Consulta [docs/RELEASE_CHECKLIST.md](docs/RELEASE_CHECKLIST.md) antes de considerarlo publicado.
