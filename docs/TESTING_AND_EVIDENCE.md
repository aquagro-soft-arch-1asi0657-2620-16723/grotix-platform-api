# Pruebas y evidencias — Capítulo V

Conforme al enunciado de Fundamentos de Arquitectura de Software: las evidencias de implementación, pruebas, ejecución, OpenAPI, despliegue y colaboración se registran por Sprint. Conservar las capturas, commits y grabaciones reales al actualizar el informe.

## Suite de pruebas existente

| Suite | Proyecto | Archivo Gherkin | Dominio |
| --- | --- | --- | --- |
| Profiles | `src/Profiles.Api.Tests` | `Acceptance/Features/Profiles.feature` | perfiles y contratos |
| Cultivation Area | `src/CultivationArea.Api.Tests` | `Acceptance/Features/CultivationArea.feature` | zonas y cultivos |
| Telemetry | `src/Telemetry.Api.Tests` | `Acceptance/Features/TelemetryIngest.feature` | rangos, promedio, umbrales |
| Hardware Device | `src/HardwareDevice.Api.Tests` | `Acceptance/Features/Hardware.feature` | mantenimiento y vinculación |
| Irrigation Cycle | `src/IrrigationCycle.Api.Tests` | `Acceptance/Features/Irrigation.feature` | riego manual, automático, cancelación |

**Advertencia sobre calidad:** algunas pruebas unitarias incluidas en la entrega original prueban objetos anónimos o mocks y no validan flujos integrados, por lo que no sustituyen pruebas reales del sistema. No marcar esas funcionalidades como completamente verificadas solo porque pasa `dotnet test`.

## Comandos de compilación y pruebas reproducibles

```bash
dotnet --info
dotnet restore Grotix-backend.sln
dotnet build Grotix-backend.sln -c Release --no-restore
dotnet test Grotix-backend.sln -c Release --no-build --logger 'trx;LogFileName=results.trx'
# Una suite específica:
dotnet test src/IrrigationCycle.Api.Tests/IrrigationCycle.Api.Tests.csproj -c Release
# Validación de estáticos:
python3 scripts/validate_deployment.py
# Smoke HTTP contra gateway (tras desplegar):
bash scripts/smoke-test.sh https://api.tudominio.com
```

Para certificar los errores mencionados por el grupo, correr cada suite de `Telemetry`, `IrrigationCycle`, `HardwareDevice` y `CultivationArea` con salida TRX, incluir los errores reales si persisten y corregirlos antes del informe. **Esta auditoría no pudo ejecutar .NET en el entorno de generación, por lo que no afirma suites aprobadas.**

## Endpoints públicos / contratos OpenAPI

El gateway preserva las rutas existentes `/api/v1/...`; sus controladores no se renombraron. Si se activa Swagger en los servicios internos (`SWAGGER_ENABLED=true`), el gateway expone:

- `/docs/profiles/openapi.json`
- `/docs/cultivation/openapi.json`
- `/docs/telemetry/openapi.json`
- `/docs/hardware/openapi.json`
- `/docs/irrigation/openapi.json`

La descripción de verbos, parámetros, códigos de error y bodies la generan los controladores existentes; **comprobar cada JSON en servidor**. La API Python Crop Analysis no se documenta como disponible hasta desplegar el servicio externo.

## Matriz mínima de evidencias

| Requisito | Evidencia requerida | Estado antes del despliegue |
| --- | --- | --- |
| Desarrollo | PR / commits por tarea, mensajes Conventional Commits, capturas de GitHub Actions | Pendiente de push real |
| Pruebas | Salida `dotnet test` y TRX por BC, escenarios `.feature` | Definidas, ejecución no verificada aquí |
| Ejecución | Postman: POST/GET con body y response, video del flujo telemetría→riego | Requiere APIs/BD/broker activos |
| Documentación | Swagger/OpenAPI JSON, endpoints por BC y captura con request/respuesta | Rutas proxy configuradas; requieren servicio activo |
| Deployment | Dockerfiles, Docker Compose, HTTPS, NSG, `docker compose ps`, URL pública | Configuración preparada; no desplegado |
| Colaboración | GitHub Insights, commits por integrante, historia de PR | Requiere evidencias GitHub del grupo |

## Flujo de riego para demostración

Para T59, T63 y T64 preparar un dispositivo/sensor y zona **reales de QA**. Registrar: evento de ingesta (request), API response, procesamiento de telemetría, decisión de riego, creación de ciclo y resultado de ejecución. Si `RabbitMq__Enabled=false`, demostrar solo HTTP y NO afirmar que la orquestación asíncrona fue validada.

## Capturas y orden para el informe

1. `dotnet --info` y build sin errores (SDK, fecha y commit).
2. Resultados `dotnet test` separados por suite, con nombres y totales reales.
3. `docker compose ps` y `docker compose logs` sin secretos.
4. Postman o curl autenticado contra URL HTTPS pública y response.
5. OpenAPI documentado en `/docs/.../openapi.json`.
6. GitHub Actions CI, release de imágenes y logs de despliegue.
7. Tabla repositorio / branch / commit SHA / Conventional Commit / fecha / responsables, más GitHub Insights.

No sustituir pruebas ejecutadas por mockups/capturas fabricadas.
