# Contenido y alcance del paquete

Se entregan todos los proyectos C# originales `src/` + `Grotix/`, configuración de solución `Grotix-backend.sln`, cinco suites de pruebas, migraciones EF Core, seis Dockerfiles de APIs, Docker Compose para APIs/bases/migrador, HTTPS Caddy y workflows GitHub Actions.

## Archivos de entrada

- `START_HERE.md`: instrucciones inmediatas.
- `.env.local.example`: Docker local, MySQL loopback 3307 y PostgreSQL loopback 5433.
- `.env.azure-vm.example`: Azure VM de una sola instancia, HTTPS y BD privadas.
- `.env.example`: alternativa de conexiones a bases administradas externas.
- `scripts/init-env.ps1` / `scripts/init-env.sh`: genera `.env` con claves aleatorias sin sobrescribir archivos existentes.
- `docker-compose.yml`: 6 APIs, Gateway/Caddy TLS.
- `docker-compose.databases.yml`: MySQL y TimescaleDB.
- `docker-compose.migrations.yml`: ejecución EF Core desde Docker (perfil temporal).
- `deploy/migrator/`: Dockerfile + entrypoint del migrador.
- `scripts/deploy-release.sh`: despliegue manual de imágenes versionadas GHCR.
- `.github/workflows/ci.yml`, `publish-images.yml`, `deploy-azure-vm.yml`: pruebas, imágenes, despliegue.

**No contiene:** entorno `.env` ni datos de las bases; imágenes Docker preconstruidas; infraestructura Azure; dominio, IP pública, SSH keys, certificado TLS; módulo Python Crop Analysis.

> Este paquete está **preparado para despliegue**. El despliegue no está consumado hasta provisionar la infraestructura, configurar secretos/domino, aplicar migraciones y probar HTTPS/operaciones REST sobre Azure.
