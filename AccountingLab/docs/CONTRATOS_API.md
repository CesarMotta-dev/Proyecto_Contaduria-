# Contratos de API

La API local utiliza `http://localhost:5208`. OpenAPI en `/swagger/v1/swagger.json` describe el contrato implementado y debe mantenerse sincronizado con esta guía. Los ejemplos pertenecen a empresas ficticias. La política académica está en [AGENTS.md](../AGENTS.md).

## Identidad, empresa y formatos

`POST /api/auth/login` recibe `{ "email": "docente@accountinglab.local", "password": "…" }`. La contraseña proviene de la configuración local. Devuelve `accessToken`, `user` con `id`, `name`, `email`, y `companies` con `id`, `name`, `currency`, `role`.

Las rutas de negocio reciben `Authorization: Bearer <accessToken>` y `X-Company-Id: <empresa autorizada>`. El servidor verifica membresía y rol; cambiar el encabezado no concede acceso. Los identificadores son UUID. Las fechas contables usan `YYYY-MM-DD`; la auditoría utiliza UTC.

Las colecciones paginadas usan `page` y `pageSize` y devuelven `{items,total,page,pageSize}`. El catálogo de desafíos devuelve un array. Los importes y tasas viajan como cadenas decimales con punto. Un importe como `"100.0000"` conserva su valor; la precisión monetaria superior a cuatro decimales se rechaza.

## Operaciones básicas

| Recurso | Operaciones | Alcance |
|---|---|---|
| `/api/companies` | GET, POST | Empresas y perfiles como datos |
| `/api/users` | GET, POST | Usuarios y soporte de acceso según permisos |
| `/api/accounts` | GET, POST | Catálogo y agrupaciones almacenadas |
| `/api/periods` | GET, POST | Periodos abiertos |
| `/api/third-parties` | GET, POST | Terceros ficticios |
| `/api/cost-centers` | GET, POST | Centros de costos |
| `/api/tax-rules` | GET, POST | Reglas y tasas ficticias como datos |
| `/api/journal-entries` | GET, POST | Asientos manuales completos |
| `/api/ledger` | GET | Movimientos y saldo por cuenta |
| `/api/receivables` | GET, POST | Obligaciones por cobrar |
| `/api/payables` | GET, POST | Obligaciones por pagar |
| `/api/payments` | GET, POST | Cobros y pagos borrador |
| `/api/invoices` | GET, POST | Facturas borrador |
| `/api/invoices/{id}` | PUT | Edición permitida de borradores |
| `/api/bank-statements` | GET, POST | Extractos en formato interno |
| `/api/bank-reconciliation` | GET, POST | Sesiones de conciliación borrador |
| `/api/budgets` | GET, POST | Presupuestos borrador |
| `/api/budgets/{id}` | PUT | Edición permitida de borradores |
| `/api/approvals` | GET, POST | Solicitudes borrador |
| `/api/notifications` | GET | Avisos iniciales etiquetados |
| `/api/audit` | GET | Hechos básicos confirmados |
| `/api/reports/summary` | GET | Resumen real de movimientos |
| `/api/exports/accounts` | GET, CSV | Exportación simple de cuentas autorizadas |
| `/api/exports/journal-entries` | GET, CSV | Exportación simple de asientos autorizados |
| `/api/challenges` | GET | Inventario y disponibilidad declarada de los 22 desafíos |

Los campos particulares de cada alta se consultan en el esquema OpenAPI. Las operaciones de borrador no confirman cobros, pagos ni procesamiento avanzado de facturas.

## Ejemplo de asiento manual

`POST /api/journal-entries` recibe un identificador de operación, referencia, fecha y líneas. Los UUID del ejemplo deben sustituirse por cuentas de movimiento válidas de la empresa activa.

```json
{
  "operationId": "33333333-3333-4333-8333-333333333333",
  "reference": "ASIENTO-DEMO-001",
  "date": "2026-10-23",
  "lines": [
    { "accountId": "44444444-4444-4444-8444-444444444444", "debit": "100.0000", "credit": "0.0000", "costCenterId": null },
    { "accountId": "55555555-5555-4555-8555-555555555555", "debit": "0.0000", "credit": "100.0000", "costCenterId": null }
  ]
}
```

El periodo debe estar abierto y las cuentas activas deben pertenecer a la empresa. Un documento válido se conserva completo y se refleja en diario, mayor y resumen. Un rechazo no deja cabecera, líneas ni cambios de saldo parciales. La repetición del identificador con el mismo contenido conserva el resultado; contenido diferente produce conflicto. Este registro manual no aplica cobros/pagos a obligaciones ni revierte comprobantes.

## Capacidades académicas

`GET /api/reports/balance-sheet` y `POST /api/accounting-periods/close` conservan las capacidades avanzadas pendientes. La conciliación GET básica consulta sesiones; no ejecuta la conciliación. La matriz registra los casos avanzados de cada módulo y su DP.

`POST /api/challenges/{DP-XX}/execute` ofrece además un acceso educativo al punto pendiente real de cada desafío; no devuelve resultados inventados. La guía individual y OpenAPI definen los datos que admite. La revisión de integración debe conservar el enlace entre la ruta, la zona pendiente y sus pruebas directas.

Una petición admitida que alcanza su pendiente devuelve 501, con `challengeId`, `patternName`, `referenceUrl` y correlación. Esa respuesta no crea documentos, movimientos, cambios de estado ni auditoría de éxito. Las pruebas base del transporte pueden comprobar el 501; las pruebas académicas esperan el comportamiento final del ejercicio y fallan inicialmente.

## Errores

| Estado | Significado |
|---|---|
| 400 | Formato, precisión o datos de entrada inválidos |
| 401 | Identidad ausente o inválida |
| 403 | Permisos o membresía insuficientes |
| 404 | Recurso inexistente en el alcance autorizado |
| 409 | Conflicto de identidad de operación o concurrencia |
| 501 | Contrato académico explícitamente pendiente |
| 500 o error de disponibilidad | Fallo inesperado o de entorno; no acredita un pendiente académico |

Los errores son estructurados, con estado, código estable, explicación y correlación. Las validaciones y permisos previos conservan sus códigos aunque la capacidad final esté pendiente. No se transforman excepciones inesperadas en 501.

## Salud y evidencia

`GET /health/live` comprueba vida del proceso. `GET /health/ready` comprueba disponibilidad de MySQL y esquema y determina el estado saludable del contenedor API. Estas rutas no acreditan por sí mismas los veinte recorridos operativos.

La disponibilidad declarada en el catálogo no equivale a pruebas aprobadas. Los reportes realmente ejecutados y las limitaciones se registran en [ENTREGA](ENTREGA.md).
