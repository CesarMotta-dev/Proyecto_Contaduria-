# DP-15 — Iterator

## Problema y contexto empresarial

Recorrer libros entregados en grupos o páginas sin que el consumidor conozca su almacenamiento.

## Situación inicial

El contrato existe y compila. Su cuerpo conserva el marcador `TODO(STUDENT): DP-15` y lanza un fallo académico identificado. El comportamiento completo queda reservado al estudiante; el soporte básico de la plataforma no lo proporciona.

## Contrato y punto de extensión

- Capa: Application.
- Archivo: [Challenge.cs](../../../src/AccountingLab.Application/Challenges/DP-15_ITERATOR/Challenge.cs).
- Contrato público: `LedgerTraversalChallenge.Open(IReadOnlyList<LedgerPage> pages)`.
- Pruebas: [ChallengeTests.cs](../../../tests/AccountingLab.PatternChallenges/DP-15_ITERATOR/ChallengeTests.cs).

Los tipos públicos de entrada y salida describen datos observables. No constituyen los participantes internos de una solución y no obligan a copiar una implementación externa. Las precondiciones de cada caso están en sus datos de prueba.

## Trabajo pendiente

El estudiante analiza el problema y desarrolla una solución propia que satisfaga el contrato y la intención de Iterator. Debe justificar responsabilidades, dependencias, extensibilidad y límites. Ninguna guía inicial contiene la colaboración resuelta ni pseudocódigo.

## Criterios de aceptación observables

El recorrido conserva orden y no omite registros entre páginas; una colección vacía termina sin elementos; Current falla antes del primer elemento y después del final. Las páginas vacías intermedias no generan registros ficticios.

Además de los resultados funcionales, la revisión docente valora la correspondencia entre el problema, la intención del patrón y la estructura elegida. Una clase con el nombre del patrón no acredita el ejercicio.

## Pruebas académicas

Este desafío tiene tres casos independientes bajo el trait `ChallengeId=DP-15`. Se ejecutan sin servicios externos reales. Esperan el comportamiento final y fallan inicialmente al alcanzar el pendiente propio; no aprueban por obtener una excepción de funcionalidad pendiente.

Desde la raíz de AccountingLab:

```powershell
.\scripts\Invoke-Tests.ps1 -Category Academic -ChallengeId DP-15
```

El fallo inicial es evidencia del trabajo aún reservado al estudiante. No se deben alterar aserciones, omitir casos ni convertir el resultado fallido en una aprobación artificial.

## Preguntas conceptuales

- ¿Qué necesita conocer quien recorre el libro y qué detalles del almacenamiento deberían ser irrelevantes?
- ¿Qué evidencia funcional aportan las pruebas y qué decisiones de diseño necesitan revisión adicional?
- ¿Qué acoplamientos introduces y cómo explicarías sus ventajas y limitaciones?

## Referencia y política académica

[Iterator en Refactoring.Guru](https://refactoring.guru/es/design-patterns/iterator). La referencia ofrece contexto conceptual; no se incorporan sus soluciones al ejercicio.

La [política académica de AGENTS.md](../../../AGENTS.md) reserva la implementación al estudiante. Un asistente puede explicar conceptos, mensajes de error y pruebas, y formular preguntas; no puede completar este desafío ni entregar una solución equivalente.

