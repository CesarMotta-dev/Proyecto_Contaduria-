# Actividad individual: AccountingLab

La empresa ficticia Laboratorio Contable gestiona operaciones de varias empresas de demostración. El sistema ofrece una base operativa y veintidós contratos avanzados que el estudiante desarrollará aplicando el catálogo de [Refactoring.Guru](https://refactoring.guru/es/design-patterns/catalog).

La ayuda de agentes se rige por [AGENTS.md](../AGENTS.md) y [POLITICA_USO_IA](POLITICA_USO_IA.md). Las guías describen problemas y criterios observables; la implementación y el UML de cada solución pertenecen al estudiante.

## Objetivos

Reconocer la intención de cada patrón, justificar su uso en el negocio, mantener las dependencias de arquitectura limpia y demostrar el resultado con pruebas y una explicación propia. OOP, SOLID, exactitud monetaria y consistencia transaccional acompañan esa evaluación.

La actividad se organiza en tres módulos académicos: cinco patrones creacionales, siete estructurales y diez de comportamiento. El docente fija el calendario y las entregas por grupos pequeños. Veintidós desafíos requieren trabajo sostenido; el alcance no presupone una actividad de dos sesiones.

## Base operativa y trabajo reservado

| Área básica | Recorrido disponible | Capacidad académica reservada |
|---|---|---|
| Empresas | Registro y consulta de perfiles como datos | Servicios por perfil |
| Usuarios | Autenticación, membresías y roles | La autorización base permanece activa |
| Cuentas | Catálogo y vínculos de agrupación | Operaciones uniformes de jerarquía |
| Asientos | Registro manual completo y balanceado | Comprobantes por tipo y solicitudes avanzadas |
| Diario | Listas filtradas y paginadas | Recorrido abstracto |
| Mayor | Movimientos y saldo por cuenta | Recorrido y análisis avanzados |
| Terceros | Clientes y proveedores ficticios | Integraciones externas fuera de alcance |
| Cuentas por cobrar | Obligaciones y cobros borrador | Aplicación de cobros |
| Cuentas por pagar | Obligaciones y pagos borrador | Aprobación y aplicación de pagos |
| Facturación | Borradores editables | Procesamiento y ciclo de vida |
| Impuestos | Reglas ficticias y vigencias como datos | Cálculo configurable |
| Conciliación | Extractos internos y sesiones borrador | Importación bancaria y conciliación |
| Centros de costos | Registro y asociación manual | Repartos automáticos fuera de la base |
| Presupuestos | Borradores editables | Instantáneas y restauración |
| Reportes | Resumen de movimientos reales | Configuración, representación y acceso intermediado |
| Auditoría | Hechos básicos confirmados | Análisis adicional |
| Notificaciones | Avisos iniciales identificados como demostración | Suscripciones y entrega |
| Exportación | CSV simple autorizado | Exportación enriquecida |
| Aprobaciones | Solicitudes borrador | Políticas de aprobación |
| Periodos | Periodos abiertos y consulta | Cierre contable |

El estado comprobado de cada recorrido está en [ENTREGA](ENTREGA.md). Un pendiente visible no representa una operación completada.

## Reglas contables del laboratorio

- Cada empresa utiliza una moneda de referencia y mantiene datos aislados de otras empresas.
- Un asiento manual tiene al menos dos líneas de cuentas activas de movimiento de su empresa y un periodo abierto.
- Cada línea contiene un débito o un crédito positivo; los totales deben coincidir.
- Los importes admiten hasta cuatro posiciones decimales. En HTTP se transportan como cadenas decimales con punto; el exceso de precisión se rechaza.
- Las tasas son ficticias y configurables. No representan obligaciones tributarias reales.
- Las confirmaciones financieras son indivisibles y los reintentos no duplican movimientos.
- Los hechos contabilizados conservan su historia. Las correcciones avanzadas requieren una reversión vinculada; siguen pendientes mientras no se resuelva su contrato.
- La restauración histórica de DP-17 se limita a presupuestos borrador.

Como ejemplo de la base, un asiento manual puede registrar un débito de `"100.0000"` y un crédito de `"100.0000"` en dos cuentas válidas de la misma empresa. El saldo consultado debe corresponder a esos movimientos reales. El estudiante obtiene los identificadores válidos de la API o los datos iniciales.

## Desarrollo y evidencia individual

Consulta [el catálogo](CATALOGO_22_PATRONES.md), [la matriz](MATRIZ_TRAZABILIDAD.md) y la guía individual del DP. Ejecuta sus pruebas para comprender el contrato y conserva la salida inicial. Desarrolla tu solución por incrementos y documenta las decisiones propias; los tests funcionales no sustituyen la justificación del diseño.

Por cada desafío entrega una explicación del problema, elección del patrón, participantes de tu solución, responsabilidades, ventajas, límites, UML propio y evidencia de pruebas. Mantén los contratos y la arquitectura; las pruebas existentes no se reescriben para obtener aprobación.

Preguntas de reflexión: ¿qué cambio del negocio justifica este patrón?, ¿qué responsabilidades permanecen estables?, ¿qué evidencia comprueba el contrato?, ¿qué costo introduce el diseño?, ¿cómo defenderías esa decisión ante una alternativa?

La rúbrica está en [CRITERIOS_EVALUACION](CRITERIOS_EVALUACION.md). El avance legítimo puede convertir fallos académicos en pruebas aprobadas; no existe una cuota permanente de fallos.
