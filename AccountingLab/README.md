# AccountingLab

Laboratorio universitario de contabilidad con API .NET 8, MySQL 8.4 y web React/TypeScript. Incluye veinte módulos básicos y veintidós desafíos académicos reservados al estudiante. La política aplicable está en [AGENTS.md](AGENTS.md) y [POLITICA_USO_IA](docs/POLITICA_USO_IA.md).

Consulta [ENTREGA](docs/ENTREGA.md) para revisar comandos ejecutados, resultados y reportes de la preparación docente.

## Recorrido inicial

1. Lee [la actividad](docs/ACTIVIDAD_ESTUDIANTES.md), [la arquitectura](docs/ARQUITECTURA.md) y [los criterios de evaluación](docs/CRITERIOS_EVALUACION.md).
2. Prepara la configuración local y arranca los contenedores.
3. Accede a la web, registra un asiento manual y consulta su reflejo en los libros.
4. Consulta [el catálogo](docs/CATALOGO_22_PATRONES.md) y [la matriz de trazabilidad](docs/MATRIZ_TRAZABILIDAD.md). Una operación avanzada pendiente debe identificarse con su DP.
5. Ejecuta las pruebas por categoría y conserva los reportes antes de comenzar tu solución individual.

## Requisitos

| Uso | Requisitos |
|---|---|
| Entorno contenedorizado | Docker con Compose v2 y puertos locales 5173, 5208 y 3308 disponibles |
| Pruebas backend en el host | SDK .NET 8, PowerShell 7 y MySQL desechable en puerto 3309 |
| Frontend fuera del contenedor | Node 22, revisión 22.12 o superior, y npm |

El SDK local, si existe, se encuentra en `.tools/dotnet/`; los scripts lo prefieren al instalado en PATH. `global.json` y los manifiestos registran las versiones del proyecto. El tag MySQL inicial es `mysql:8.4`; el informe de ejecución debe registrar la revisión o digest efectivamente utilizado antes de acreditar reproducibilidad de las imágenes.

Revisiones fijadas: EF Core y autenticación ASP.NET Core 8.0.32, proveedor Pomelo 8.0.3, React 19.1.1, TypeScript 5.8.3, Vite 7.3.7, Tailwind 4.1.18 y React Router 7.18.4. Los archivos `packages.lock.json` y `frontend/package-lock.json` conservan la resolución de dependencias.

## Configuración y arranque con Docker

Desde la raíz `AccountingLab/`:

```powershell
if (-not (Test-Path -LiteralPath .env)) { Copy-Item .env.example .env }
```

Edita `.env`: sustituye los marcadores ficticios de contraseñas y `JWT_SIGNING_KEY`, que requiere al menos 32 caracteres. Para este recorrido utiliza valores locales sin punto y coma, ya que la conexión se construye como cadena MySQL. `.env` no se incluye en las imágenes. Conserva la contraseña demo para iniciar sesión.

```powershell
docker compose --env-file .env config --quiet
docker compose --env-file .env up --build -d --wait mysql api frontend
docker compose --env-file .env ps --all
```

El proyecto Compose se llama `accountinglab-actividad3`. MySQL utiliza un volumen propio; `migrate` aplica las migraciones y los datos ficticios una sola vez por ejecución y termina. La API espera su salida correcta y comprueba base y esquema mediante `/health/ready`.

| Servicio | Dirección local |
|---|---|
| Web | <http://localhost:5173> |
| API | <http://localhost:5208> |
| Swagger | <http://localhost:5208/swagger> |
| Vida del proceso | <http://localhost:5208/health/live> |
| Disponibilidad de base y esquema | <http://localhost:5208/health/ready> |
| MySQL demo | `127.0.0.1:3308`, base `accountinglab` por defecto |

Para detener los servicios conservando el volumen:

```powershell
docker compose --env-file .env down
```

Las guías de [migraciones](database/migrations/README.md) y [datos iniciales](database/seed/README.md) explican el origen único del esquema y la repetición del inicializador. Los cambios de configuración no deben borrar trabajo existente.

## Autenticación y empresa activa

El correo demo predeterminado es `docente@accountinglab.local`; la contraseña procede de `DEMO_PASSWORD`. El login devuelve las empresas autorizadas. Las rutas de negocio requieren `Authorization: Bearer …` y `X-Company-Id`.

```powershell
$loginBody = @{ email = 'docente@accountinglab.local'; password = (Read-Host 'Contraseña demo' -MaskInput) } | ConvertTo-Json
$session = Invoke-RestMethod -Method Post -Uri 'http://localhost:5208/api/auth/login' -ContentType 'application/json' -Body $loginBody
$requestHeaders = @{ Authorization = "Bearer $($session.accessToken)"; 'X-Company-Id' = $session.companies[0].id }
Invoke-RestMethod -Uri 'http://localhost:5208/api/accounts' -Headers $requestHeaders
```

Los usuarios solo acceden a empresas de su membresía. Administrador, Contador y Consulta tienen permisos diferenciados. Consulta [CONTRATOS_API](docs/CONTRATOS_API.md) y OpenAPI para las operaciones disponibles.

## Ejecución directa del backend

Configura `ConnectionStrings__AccountingLab` para MySQL en el puerto 3308, `Demo__Email`, `Demo__Password`, `Authentication__SigningKey`, `Authentication__Issuer=AccountingLab` y `Authentication__Audience=AccountingLab.Web`. Las claves se proporcionan desde el entorno, sin incluirlas en informes.

```powershell
dotnet restore AccountingLab.sln
dotnet run --project src/AccountingLab.Api -- --migrate-only
dotnet run --project src/AccountingLab.Api --urls http://localhost:5208
```

Si utilizas el SDK local de Windows, invoca `.tools/dotnet/dotnet.exe` en lugar de `dotnet`, o añade esa carpeta a tu PATH de sesión. Solo el proceso `--migrate-only` aplica esquema y datos; evita dos responsables simultáneos de migración sobre la misma base.

## Pruebas y reportes

Las pruebas de integración requieren una base desechable distinta de la demo. El perfil `tests` prepara MySQL en `127.0.0.1:3309` con almacenamiento temporal:

```powershell
docker compose --env-file .env --profile tests up -d --wait mysql-test
$testPassword = Read-Host 'MYSQL_TEST_PASSWORD configurada en .env' -MaskInput
$env:ConnectionStrings__AccountingLabTest = "Server=127.0.0.1;Port=3309;Database=accountinglab_tests;User=accountinglab_test;Password=$testPassword;"
```

Si cambiaste nombre o usuario de pruebas en `.env`, utiliza esos mismos valores. Nunca asignes la conexión de demostración a la variable de pruebas.

```powershell
pwsh -NoProfile -File scripts/Invoke-Tests.ps1 -Category Base
pwsh -NoProfile -File scripts/Invoke-Tests.ps1 -Category Architecture
pwsh -NoProfile -File scripts/Invoke-Tests.ps1 -Category Academic
pwsh -NoProfile -File scripts/Invoke-Tests.ps1 -Category Academic -ChallengeId DP-03
```

La selección por DP es una ejecución parcial. Para el recorrido completo, instala las dependencias frontend y ejecuta:

```powershell
npm --prefix frontend ci
pwsh -NoProfile -File scripts/Invoke-Tests.ps1 -Category All -Delivery
```

Los scripts conservan TRX, consola, conteos y código de salida en un directorio nuevo de `artifacts/test-results/`. Base y arquitectura deben aprobar. La entrega docente debe mostrar al menos 66 fallos académicos propios, tres por DP, sin omisiones. El resultado completo conserva su salida fallida inicial; esa cifra no se impone después del avance legítimo del estudiante.

La comprobación de integridad tiene dos modalidades:

```powershell
pwsh -NoProfile -File scripts/Test-Integrity.ps1 -Delivery
pwsh -NoProfile -File scripts/Test-Integrity.ps1
```

`-Delivery` verifica además los marcadores pendientes iniciales. El modo normal conserva la trazabilidad sin exigir que el alumno mantenga TODOs o fallos. Ninguno acredita por sí solo la corrección de un patrón ni la ausencia de soluciones encubiertas.

Para comprobar las 22 rutas avanzadas reales, sus metadatos 501 y la conservación del estado de negocio en la entrega inicial:

```powershell
$env:Demo__Password = Read-Host 'DEMO_PASSWORD configurada en .env' -MaskInput
pwsh -NoProfile -File scripts/Test-TeacherDelivery.ps1 -IncludeEducationalRoutes
```

Esta comprobación crea una empresa ficticia de revisión y conserva un reporte JSON. Se ejecuta exclusivamente al preparar la entrega docente; después de resolver los ejercicios, las rutas podrán devolver resultados funcionales. No forma parte de la evaluación de corrección de los patrones.

## Frontend y comprobaciones de construcción

```powershell
npm --prefix frontend ci
npm --prefix frontend test
npm --prefix frontend run build
dotnet build AccountingLab.sln --configuration Release
```

El contenedor web sirve la interfaz y dirige `/api` hacia `api:8080`. Los tests de interfaz comprueban navegación, formularios y presentación de errores o pendientes; no implementan los desafíos. Los workflows mantienen trabajos independientes y adjuntan evidencia incluso cuando la categoría académica falla.

## Documentación

- [Actividad del estudiante](docs/ACTIVIDAD_ESTUDIANTES.md)
- [Arquitectura](docs/ARQUITECTURA.md)
- [Contratos de API](docs/CONTRATOS_API.md)
- [Catálogo de patrones](docs/CATALOGO_22_PATRONES.md)
- [Matriz de trazabilidad](docs/MATRIZ_TRAZABILIDAD.md)
- [Criterios de evaluación](docs/CRITERIOS_EVALUACION.md)
- [Política de IA](docs/POLITICA_USO_IA.md)
- [Estado y evidencia de entrega](docs/ENTREGA.md)
