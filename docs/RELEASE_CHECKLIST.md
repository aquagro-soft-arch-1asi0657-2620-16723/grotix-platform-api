# Checklist de entrega — Grotix Azure VM

## Validaciones completadas por el equipo en Windows (evidencias del chat)

- [x] .NET SDK 10 instalado; `dotnet restore` y `dotnet build -c Release` exitosos (11 advertencias de nulabilidad en tests).
- [x] `dotnet test ... --no-build`: 115 aprobadas, 0 fallidas.
- [x] MySQL 8 y TimescaleDB/PG16 healthy en Docker.
- [x] Aplicadas 36 migraciones de seis DbContext y comprobado snapshot de Cultivation Area con `has-pending-model-changes`: sin diferencias.
- [x] Docker construyó e inició Gateway, Profiles, Cultivation, Hardware, Telemetry, Irrigation.
- [x] GET `/live`, `/ready/core`, `/ready/telemetry` y tres health por dominio retornaron HTTP 200.
- [x] OpenAPI de Profiles/Irrigation GET 200.
- [x] Bootstrap de administrador, POST login, GET perfil JWT correctos.
- [x] Cinco GET protegidos y POST/GET/PUT/DELETE de cultivo, 204 y 404 tras borrar.

## Obligatorio antes de considerar lista la publicación Azure

- [ ] Subir/mergear **este ZIP final** como archivos, sin secretos ni backups, a ramas del repositorio.
- [ ] GitHub Actions CI con todas las suites funcionando usando el código final.
- [ ] Confirmar que `.env` real de la VM tiene secretos aleatorios y distintos de los compartidos antes; rotación completada.
- [ ] Elegir y provisionar bases MySQL y TimescaleDB, ejecutar migraciones sobre instancia Azure y verificar readiness.
- [ ] Crear VM/NSG/IP pública, habilitar SSH restringido y DNS A correcto.
- [ ] Habilitar 80/443 y validar certificados HTTPS de Caddy (nunca exponer 3306/5432).
- [ ] Realizar release inmutable/tag en GHCR, Pull Request aprobado y Secrets/Environment GitHub seguros.
- [ ] Validar acceso externo real: health, JWT, autorización y un CRUD de prueba.
- [ ] Probar backup/restore, revisar warnings/logs y plan de rollback.
- [ ] Confirmar requisito funcional de RabbitMQ/Crop Analysis con el equipo antes de la demo.

## Limitaciones de esta consolidación

Este ZIP se compiló conceptualmente a partir del código que pasó 115 tests en el equipo del usuario. Los **scripts nuevos** para Docker migrator, generadores `.env` y SSH CI/CD no se han ejecutado todavía en una VM Azure real ni en este contenedor de preparación (aquí no hay Docker daemon/.NET SDK). Se verifica su sintaxis/estructura; la puesta en marcha real sigue siendo necesaria. No entregar como prueba de despliegue ni afirmar que es production-grade.
