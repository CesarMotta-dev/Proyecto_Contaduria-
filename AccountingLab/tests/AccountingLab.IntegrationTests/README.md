# Pruebas de integración MySQL y API

Estas pruebas usan `WebApplicationFactory` y MySQL real. Requieren `ConnectionStrings__AccountingLabTest` con una base desechable cuyo nombre contenga `test`; nunca ejecutan `EnsureDeleted` ni limpian la base global. Cada escenario crea una empresa y referencias propias. La ausencia de conexión o de base disponible produce un error explícito, sin omisiones.

El fixture aplica migraciones EF y `DemoSeed.ApplyAsync`. La cuenta docente usa `docente@accountinglab.local` y una contraseña exclusiva de pruebas; si la base de pruebas conserva otra credencial previa, configura `AccountingLabTest__DemoPassword` con esa misma contraseña. El fixture no sobrescribe usuarios existentes.

La batería comprueba módulos básicos, transporte decimal, permisos, aislamiento, transacciones, idempotencia, concurrencia, conservación de auditoría, repetición de seed, migraciones, reinicio, salud y OpenAPI. La falla posterior a guardado es una inyección exclusiva del host de pruebas para verificar el rollback real de MySQL.

El controlador `TestPendingController` solo se añade al host de pruebas. Comprueba la traducción de `PatternPendingException` sin exigir que ningún patrón real permanezca pendiente después del trabajo del alumno. Sus casos no acreditan las pruebas académicas de los veintidós desafíos.

Las rutas reales todavía pendientes se comprueban separadamente con `scripts/Test-TeacherDelivery.ps1`, exclusivamente para acreditar el estado docente inicial. Ese script conserva un informe JSON y compara los datos de una empresa ficticia nueva antes y después de cada petición. No forma parte de la suite funcional permanente del estudiante y no impone fallos o pendientes después de su progreso.

La política académica permanece en [AGENTS.md](../../AGENTS.md).
