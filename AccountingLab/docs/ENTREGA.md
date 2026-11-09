# Informe de preparación docente

Verificado el **23 de octubre de 2026**, America/Bogota. Backend, web, persistencia, documentación y evaluación inicial están preparados y ejecutados. **Los 22 patrones continúan sin implementar.**

## Estructura creada

    AccountingLab/
      src/AccountingLab.Domain/
      src/AccountingLab.Application/
      src/AccountingLab.Infrastructure/
      src/AccountingLab.Api/
      frontend/src/{components,pages,services,hooks,types}/
      tests/AccountingLab.UnitTests/
      tests/AccountingLab.IntegrationTests/
      tests/AccountingLab.ArchitectureTests/
      tests/AccountingLab.PatternChallenges/
      docs/challenges/
      database/{migrations,seed}/
      scripts/
      reports/
      .github/workflows/verify.yml
      AGENTS.md
      README.md
      AccountingLab.sln
      Dockerfile.api
      docker-compose.yml
      .env.example

El frontend incluye su propio Dockerfile. Las migraciones C# y el snapshot están en Infrastructure; InitialAccounting.sql es una exportación generada.

## Construcción y entorno comprobados

| Comprobación | Resultado y evidencia |
|---|---|
| Herramientas del host | .NET SDK **8.0.426**, Node **22.14.0**, npm **10.9.2**, Compose **5.6.0** |
| Dependencias backend | EF Core y autenticación ASP.NET Core **8.0.32**, Pomelo **8.0.3**; resoluciones en packages.lock.json |
| Dependencias web | React **19.1.1**, TypeScript **5.8.3**, Vite **7.3.7**, Tailwind **4.1.18**, React Router **7.18.4**; package-lock.json versionado |
| MySQL real | **8.4.13**; imagen mysql@sha256:76f0358884cb92d9a086d24e5671c38048d3fef40f2588703d73eb55392d6b69 |
| Backend Release | Salida **0**, cero advertencias y errores; [consola](../reports/build-backend-verified.log) |
| Frontend TypeScript y Vite | Salida **0**; [consola](../reports/build-frontend.log) |
| Contenedores | API y frontend construidos, salida **0**; [construcción](../reports/build-containers.log), [API definitiva](../reports/build-api-verified.log) |
| Arranque Compose | Salida **0**, migrador finalizado y API saludable; [consola](../reports/compose-start.log), [servicios](../reports/compose-services.txt) |
| Esquema | Migración **20261023054030_InitialAccounting**, versión EF **8.0.32**, aplicada sobre MySQL; [historial real](../reports/mysql-migrations.txt) |
| Inicialización fallida | Migración con credenciales ficticias inválidas: código **139**, sin anunciar disponibilidad; [resultado](../reports/migration-failure.json). La prueba de integración de base inexistente exige readiness **503** |
| Salud y OpenAPI | Vida, disponibilidad, Swagger y web: HTTP **200**; **53 rutas** OpenAPI; [resultado](../reports/http-readiness.json) |
| Seed repetible y reinicio | Pruebas de conservación de datos y del historial de migraciones aprobadas dentro del TRX de integración |

Comandos de construcción comprobados:

    dotnet build AccountingLab.sln --configuration Release --no-restore --no-incremental --disable-build-servers
    npm --prefix frontend run build
    docker compose build api frontend
    docker compose up -d --wait mysql api frontend

La integración utiliza MySQL real separado en el puerto 3309; la demostración persiste en el 3308. Las contraseñas proceden de .env, excluido de las imágenes y del control de versiones.

## Resultados de las pruebas

Ejecución completa, con ConnectionStrings__AccountingLabTest configurada:

    pwsh -NoProfile -File scripts/Invoke-Tests.ps1 -Category All -Delivery -ResultsDirectory reports/delivery

| Grupo | Ejecutadas | Aprobadas | Fallidas | Omitidas | Reporte original |
|---|---:|---:|---:|---:|---|
| Unitarias de soporte | 93 | 93 | 0 | 0 | [TRX](../reports/delivery/UnitTests/results.trx) |
| Integración sobre MySQL | 76 | 76 | 0 | 0 | [TRX](../reports/delivery/IntegrationTests/results.trx) |
| Arquitectura | 6 | 6 | 0 | 0 | [TRX](../reports/delivery/ArchitectureTests/results.trx) |
| Frontend | 25 | 25 | 0 | 0 | [JSON](../reports/delivery/Frontend/results.json) |
| Desafíos académicos | 66 | 0 | 66 | 0 | [TRX](../reports/delivery/PatternChallenges/results.trx) |
| **Total** | **266** | **200** | **66** | **0** | [Resumen](../reports/delivery/SUMMARY.md) |

La salida global es **1**, conservada por los fallos académicos esperados. Los 66 casos alcanzan PatternPendingException de su propio DP, tres por identificador; [inspección de mensajes y conteos](../reports/academic-audit.json). No hay fallos de entorno en esta ejecución. Los casos esperan comportamiento funcional: podrán aprobar tras el trabajo del estudiante.

Las carpetas reports/final y reports/verified conservan ejecuciones preliminares. La primera registró un bloqueo entre compilaciones concurrentes; se repitió secuencialmente. El reporte que acredita los conteos anteriores es exclusivamente **reports/delivery**.

Los verificadores de integridad normal y de entrega terminaron con salida **0**. La sintaxis PowerShell del verificador HTTP docente no presenta errores. Los trabajos de CI preservan los reportes incluso ante el fallo académico inicial.

## Funciones básicas comprobadas

| Módulo | Recorrido verificado |
|---|---|
| Empresas | Alta, membresía propia y consulta sin depender del orden del seed |
| Usuarios | Login, contraseña inválida, roles, administración y rechazo 401/403 |
| Cuentas | Alta y consulta por empresa; agrupación almacenada como datos |
| Asientos | Partida doble, importes exactos, validación y registro completo |
| Libro diario | Asientos persistidos, filtros y paginación |
| Libro mayor | Movimientos reales y saldo por cuenta |
| Terceros | Alta y consulta aisladas por empresa |
| Cuentas por cobrar | Obligaciones y vencimientos persistidos |
| Cuentas por pagar | Registro y consulta de obligaciones |
| Facturación | Alta y edición de borradores; conflictos 409 y protección del estado persistido |
| Impuestos | Reglas ficticias, precisión y vigencia almacenadas; cálculo DP-20 pendiente |
| Conciliación | Extractos internos y sesiones borrador persistidas |
| Centros de costos | Alta y pertenencia a la empresa en asientos |
| Presupuestos | Alta y edición de borradores; protección contra sobrescritura posterior |
| Reportes | Resumen de movimientos e importes reales |
| Auditoría | Hechos confirmados, inmutabilidad y ausencia de auditoría de éxito ante rechazo |
| Notificaciones | Avisos iniciales identificados como demostración |
| Exportación | CSV autorizado de cuentas y asientos con importes exactos |
| Aprobaciones | Solicitudes borrador persistidas |
| Periodos | Periodos abiertos; cierre avanzado DP-10 pendiente |

El TRX de integración acredita aislamiento entre empresas, pertenencia de referencias, repetición idempotente, concurrencia y rollback de cabecera, líneas y auditoría ante error antes del commit. También cubre precisión excesiva, datos duplicados y protección de documentos que dejaron de ser borradores. El registro manual no aplica pagos a obligaciones ni revierte comprobantes DP-14.

La web se recorrió contra la API real: autenticación, alta de cuenta, persistencia tras recargar, cambio de empresa, catálogo de 22 desafíos y respuesta DP-08 pendiente. [Registro](../frontend/output/playwright/smoke-real.txt), [dashboard](../frontend/output/playwright/dashboard-real.png), [login móvil](../frontend/output/playwright/login-mobile.png), [pendiente real](../frontend/output/playwright/challenge-501-real.png).

### Rutas pendientes y ausencia de efectos

    pwsh -NoProfile -File scripts/Test-TeacherDelivery.ps1 -IncludeEducationalRoutes

Resultado: **44/44 comprobaciones aprobadas**, correspondientes a las 22 rutas de módulo y sus 22 accesos educativos. Todas devolvieron **501** con ID, nombre y referencia correctos. Se comparó antes y después de cada solicitud el estado de cuentas, periodos, terceros, centros, reglas, asientos, obligaciones, borradores, avisos, auditoría y resumen: **sin cambios**. [Reporte original](../reports/teacher-delivery.json).

Estas comprobaciones HTTP son adicionales y no cuentan entre las 66 pruebas académicas. El script es exclusivo de la entrega inicial docente y crea una empresa ficticia de revisión. La primera ejecución se interrumpió para corregir la decodificación de application/problem+json en PowerShell y usar la dirección IPv4 local; el reporte enlazado corresponde a la repetición completa correcta.

## Inventario de los 22 desafíos sin resolver

Correspondencia contrastada con el [catálogo de Refactoring.Guru](https://refactoring.guru/es/design-patterns/catalog). Cada guía contiene contexto original, contratos, criterios, punto pendiente y referencia directa. La [matriz](MATRIZ_TRAZABILIDAD.md) enlaza fuentes y pruebas.

| Familia | Desafíos |
|---|---|
| Creacionales, 5 | DP-01 Factory Method; DP-02 Abstract Factory; DP-03 Builder; DP-04 Prototype; DP-05 Singleton |
| Estructurales, 7 | DP-06 Adapter; DP-07 Bridge; DP-08 Composite; DP-09 Decorator; DP-10 Facade; DP-11 Flyweight; DP-12 Proxy |
| Comportamiento, 10 | DP-13 Chain of Responsibility; DP-14 Command; DP-15 Iterator; DP-16 Mediator; DP-17 Memento; DP-18 Observer; DP-19 State; DP-20 Strategy; DP-21 Template Method; DP-22 Visitor |

Se revisaron zonas pendientes, contratos, servicios de soporte, controladores, persistencia, dobles y frontend. Los contratos DP conservan sus excepciones explícitas, sin cuerpos funcionales ni sustitutos en TypeScript. La política de [AGENTS.md](../AGENTS.md) protege los ejercicios y permite orientación conceptual. No se modificaron las actividades anteriores. No se afirma haber creado ni ejecutado una suite privada.

## Ejecución y límites de la entrega

Desde AccountingLab, configura .env según [README](../README.md) y ejecuta:

    docker compose up --build -d --wait mysql api frontend

Web: <http://localhost:5173>. Swagger: <http://localhost:5208/swagger>. Correo demo: docente@accountinglab.local; contraseña: DEMO_PASSWORD de .env.

Los datos tributarios son ficticios y configurables. Las capacidades avanzadas de los 22 patrones están reservadas al estudiante. Los controles de integridad y la política de IA no sustituyen la revisión docente de intención, responsabilidades y comprensión. Los workflows están preparados con trabajos y artefactos separados: se verificaron localmente sus comandos, **no se ejecutó GitHub Actions remoto**.
