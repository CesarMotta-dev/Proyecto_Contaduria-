# Catálogo de los 22 desafíos

AccountingLab incluye todos los patrones del [catálogo de Refactoring.Guru](https://refactoring.guru/es/design-patterns/catalog): cinco creacionales, siete estructurales y diez de comportamiento. Cada contrato está pendiente y tiene tres pruebas funcionales propias. La infraestructura de Clean Architecture y los servicios básicos no acreditan los ejercicios.

| ID | Familia | Patrón | Contexto | Guía |
|---|---|---|---|---|
| DP-01 | Creacional | [Factory Method](https://refactoring.guru/es/design-patterns/factory-method) | Crear comprobantes de ingreso, egreso, ajuste y traslado sin acoplar consumidores a sus tipos concretos. | [Desafío](challenges/DP-01_FACTORY_METHOD/README.md) |
| DP-02 | Creacional | [Abstract Factory](https://refactoring.guru/es/design-patterns/abstract-factory) | Obtener servicios de cálculo, validación y reporte compatibles con el perfil y régimen ficticio de una empresa. | [Desafío](challenges/DP-02_ABSTRACT_FACTORY/README.md) |
| DP-03 | Creacional | [Builder](https://refactoring.guru/es/design-patterns/builder) | Preparar informes financieros con secciones opcionales, intervalo contable y anexos seleccionados. | [Desafío](challenges/DP-03_BUILDER/README.md) |
| DP-04 | Creacional | [Prototype](https://refactoring.guru/es/design-patterns/prototype) | Duplicar plantillas recurrentes cuyos renglones y etiquetas son editables. | [Desafío](challenges/DP-04_PROTOTYPE/README.md) |
| DP-05 | Creacional | [Singleton](https://refactoring.guru/es/design-patterns/singleton) | Compartir un registro inmutable de configuración educativa dentro de un único proceso, sin datos privados de empresas. | [Desafío](challenges/DP-05_SINGLETON/README.md) |
| DP-06 | Estructural | [Adapter](https://refactoring.guru/es/design-patterns/adapter) | Importar extractos de BancoA y BancoB ficticios, cuyos JSON expresan identificaciones, fechas e importes con convenciones incompatibles. | [Desafío](challenges/DP-06_ADAPTER/README.md) |
| DP-07 | Estructural | [Bridge](https://refactoring.guru/es/design-patterns/bridge) | Variar de forma independiente el tipo de reporte financiero y su representación JSON o CSV. | [Desafío](challenges/DP-07_BRIDGE/README.md) |
| DP-08 | Estructural | [Composite](https://refactoring.guru/es/design-patterns/composite) | Consultar balances sobre cuentas individuales y agrupaciones jerárquicas de cuentas. | [Desafío](challenges/DP-08_COMPOSITE/README.md) |
| DP-09 | Estructural | [Decorator](https://refactoring.guru/es/design-patterns/decorator) | Exportar documentos con marcas de auditoría y metadatos elegidos por el usuario. | [Desafío](challenges/DP-09_DECORATOR/README.md) |
| DP-10 | Estructural | [Facade](https://refactoring.guru/es/design-patterns/facade) | Ofrecer una entrada de cierre que compruebe documentos pendientes y consistencia de saldos del periodo. | [Desafío](challenges/DP-10_FACADE/README.md) |
| DP-11 | Estructural | [Flyweight](https://refactoring.guru/es/design-patterns/flyweight) | Procesar muchos movimientos que comparten clasificación inmutable mientras conservan identidad e importe propios. | [Desafío](challenges/DP-11_FLYWEIGHT/README.md) |
| DP-12 | Estructural | [Proxy](https://refactoring.guru/es/design-patterns/proxy) | Acceder a un reporte confidencial de una empresa mediante un contrato que preserve permisos y aislamiento. | [Desafío](challenges/DP-12_PROXY/README.md) |
| DP-13 | Comportamiento | [Chain of Responsibility](https://refactoring.guru/es/design-patterns/chain-of-responsibility) | Evaluar solicitudes de pago con controles configurables de fondos, política y límite de autorización. | [Desafío](challenges/DP-13_CHAIN_OF_RESPONSIBILITY/README.md) |
| DP-14 | Comportamiento | [Command](https://refactoring.guru/es/design-patterns/command) | Representar solicitudes identificadas de aplicación de cobros/pagos a obligaciones y de reversión vinculada de comprobantes publicados. | [Desafío](challenges/DP-14_COMMAND/README.md) |
| DP-15 | Comportamiento | [Iterator](https://refactoring.guru/es/design-patterns/iterator) | Recorrer libros entregados en grupos o páginas sin que el consumidor conozca su almacenamiento. | [Desafío](challenges/DP-15_ITERATOR/README.md) |
| DP-16 | Comportamiento | [Mediator](https://refactoring.guru/es/design-patterns/mediator) | Coordinar validación, contabilización y publicación de una factura sin dependencias directas innecesarias entre componentes. | [Desafío](challenges/DP-16_MEDIATOR/README.md) |
| DP-17 | Comportamiento | [Memento](https://refactoring.guru/es/design-patterns/memento) | Guardar y recuperar versiones de presupuestos que siguen siendo borradores; los asientos publicados quedan fuera de este ejercicio. | [Desafío](challenges/DP-17_MEMENTO/README.md) |
| DP-18 | Comportamiento | [Observer](https://refactoring.guru/es/design-patterns/observer) | Avisar a interesados registrados cuando ocurre un pago o un hecho financiero ficticio. | [Desafío](challenges/DP-18_OBSERVER/README.md) |
| DP-19 | Comportamiento | [State](https://refactoring.guru/es/design-patterns/state) | Gestionar comportamiento de facturas en draft, issued, paid, cancelled y overdue según acciones y fecha contable. | [Desafío](challenges/DP-19_STATE/README.md) |
| DP-20 | Comportamiento | [Strategy](https://refactoring.guru/es/design-patterns/strategy) | Calcular impuestos educativos con alternativas percentage y fixed, reglas ficticias por operación y vigencia, sin tasas tributarias legales universales. | [Desafío](challenges/DP-20_STRATEGY/README.md) |
| DP-21 | Comportamiento | [Template Method](https://refactoring.guru/es/design-patterns/template-method) | Ofrecer conciliaciones exact y tolerance con un contrato de proceso común, dejando sus variaciones como trabajo del estudiante. | [Desafío](challenges/DP-21_TEMPLATE_METHOD/README.md) |
| DP-22 | Comportamiento | [Visitor](https://refactoring.guru/es/design-patterns/visitor) | Agregar vistas de análisis sobre datos heterogéneos de facturas, pagos y asientos sin introducir cada análisis en todos los modelos. | [Desafío](challenges/DP-22_VISITOR/README.md) |

Las guías publican problemas, contratos y criterios observables. Los estudiantes deben presentar su propia solución y su UML; este catálogo no incluye implementaciones ni diagramas de participantes resueltos. [AGENTS.md](../AGENTS.md) establece las condiciones académicas de asistencia.

El inventario legible por herramientas está en [catalog.json](challenges/catalog.json). La [matriz de trazabilidad](MATRIZ_TRAZABILIDAD.md) vincula archivos, contrato y pruebas reales.

