> **ACTUALIZACIÓN 16/11/2026:** Ver [VALIDATION_STATUS.md](VALIDATION_STATUS.md). El equipo ejecutó desde Windows 115/115 pruebas, migraciones, Gateway y CRUD. La sección original “Aún no verificado” se refiere a la auditoría inicial, no a la validación posterior; Azure permanece sin desplegar.

# Auditoría del backend Grotix — 15 noviembre 2026

## Hallazgos del ZIP entregado

| Severidad | Hallazgo | Cambio aplicado |
| --- | --- | --- |
| Crítica | URI CloudAMQP con credenciales reales incrustada en cinco `appsettings.json` | Retirada de todos los archivos versionables; usar variables de entorno. **ROTAR/REVOCAR la clave en el proveedor: ya estuvo en GitHub y sigue en historia** |
| Crítica | Creación automática de `admin@grotix.com` con contraseña fija | Bootstrap administrativo solo con email/password explícitos, en entorno externo al repo |
| Alta | JWT secret fijo/predictible en configs y `.env.example` | Eliminación de defaults, obligatoriedad de JWT de mínimo 32 caracteres ASCII; gateway/servicios coherentes |
| Alta | CORS `AllowAnyOrigin` en servicios con JWT | Lista explícita `Cors:AllowedOrigins` (frontend autorizado) |
| Alta | Docker Compose principal importaba build de carpeta `../crop-analysis/...` ausente | Quitado build inválido; Crop Analysis externo, URL configurable; documentada dependencia |
| Media | Config gateway predefinida mezclaba endpoints Azure y localhost | Valores base internos Docker, overrides de Compose, rutas OpenAPI externas |
| Alta | APIs net8 (EOS 10/11/2026), tests net10, global SDK9 | Unificado target net10 y SDK10, imágenes runtime/sdk10; EF Core 9 mantenido por proveedor MySQL Pomelo9 |
| Alta | DbContext factory duplicada (Irrigation) con `root/root` | Eliminada implementación legacy redundante, mantenida versión parametrizada |
| Media | Factories de design-time incluían contraseñas predeterminadas | Requieren variables externas; no hardcoded secrets |
| Media | TLS/gestión de imágenes CI/CD incompletos | Caddy HTTPS opcional, seis imágenes versionadas, workflow CI + publicación + despliegue manual |
| Media | Ausencia de README y checklist de evidencias | Documentación de despliegue, prueba y smoke test |
| Media | Endpoint de readiness exponía mensajes de excepción y detalles de salud de BD | Limitado a estados y nombres de checks; sin datos sensibles en respuesta HTTP |
| Media | Validación manual de tokens escribía excepciones en consola | Ahora comprueba `IsValid` y retorna null sin registrar excepción/token |

## Aún no verificado / trabajo pendiente

- No se ejecutó `dotnet restore/build/test` ni `docker compose up`: el entorno de análisis carecía de .NET SDK y Docker y no tenía salida DNS a internet. La validez de compilación y ejecución se debe confirmar en GitHub Actions o estación con SDK 10.
- No se desplegó infraestructura Azure desde esta sesión, ni se verificaron en ejecución credenciales, DNS, certificado TLS o servidor de BD. El script histórico menciona URLs de Azure App Service, pero no se comprobó su estado actual.
- No se ha provisto repositorio/imagen funcional de Crop Analysis ni del AI Service. Sus endpoints no están verificados.
- Se mantiene EF Core 9 + Pomelo9 por compatibilidad; estudiar migración a un proveedor compatible con EF Core 10 cuando esté disponible. No modificar migraciones a ciegas.
- Revisar y ejecutar migraciones para MySQL/Timescale antes de publicar. El nuevo `migrate.ps1` exige variables de entorno y confirmación explícita mediante `-Apply` (no ejecutado en este entorno).
- Revisar funcionalidad y cobertura real de pruebas: algunos tests existentes validan mocks/simulaciones en vez de API/BD. No suponer que cubren los errores listados por el grupo.
- Rotar también cualquier JWT firmada con claves públicas del ZIP, cambiar credenciales seed admin que hubieran sido utilizadas y purgar secretos históricos siguiendo procedimiento seguro si la organización lo desea. La limpieza del árbol **no** borra la historia de Git.
- Aplicar revisión específica de autorización por endpoint, riesgos de IDOR, rate limiting, observabilidad y gestión de identidades antes de usar en producción con usuarios reales.

## Principio de limpieza adoptado

Se conservan carpetas `Grotix/` y `src/`: aunque parezcan duplicadas, los `.csproj` de la solución incluyen código desde `Grotix/` usando `Compile Include` explícitos. Eliminarlas rompería proyectos. Se eliminaron únicamente un factory legacy duplicado, una copia de design-time factory no compilada, scripts de despliegue Kudu obsoletos y logs autogenerados de despliegues anteriores; también se corrigieron referencias de proyectos de pruebas y configuraciones obsoletas o inseguras; no se retiraron controladores ni agregados de dominio por conjetura.
