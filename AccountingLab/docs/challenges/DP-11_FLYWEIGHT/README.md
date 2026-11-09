# DP-11 — Flyweight

## Problema y contexto empresarial

Procesar muchos movimientos que comparten clasificación inmutable mientras conservan identidad e importe propios.

## Situación inicial

El contrato existe y compila. Su cuerpo conserva el marcador `TODO(STUDENT): DP-11` y lanza un fallo académico identificado. El comportamiento completo queda reservado al estudiante; el soporte básico de la plataforma no lo proporciona.

## Contrato y punto de extensión

- Capa: Domain.
- Archivo: [Challenge.cs](../../../src/AccountingLab.Domain/Challenges/DP-11_FLYWEIGHT/Challenge.cs).
- Contrato público: `SharedClassificationChallenge.Prepare(IReadOnlyList<MovementClassificationInput> movements)`.
- Pruebas: [ChallengeTests.cs](../../../tests/AccountingLab.PatternChallenges/DP-11_FLYWEIGHT/ChallengeTests.cs).

Los tipos públicos de entrada y salida describen datos observables. No constituyen los participantes internos de una solución y no obligan a copiar una implementación externa. Las precondiciones de cada caso están en sus datos de prueba.

## Trabajo pendiente

El estudiante analiza el problema y desarrolla una solución propia que satisfaga el contrato y la intención de Flyweight. Debe justificar responsabilidades, dependencias, extensibilidad y límites. Ninguna guía inicial contiene la colaboración resuelta ni pseudocódigo.

## Criterios de aceptación observables

Movimientos con la misma clasificación (código, etiqueta, ámbito ficticio) comparten identidad de metadatos; clasificaciones diferentes no se confunden; importes e identificaciones particulares permanecen independientes.

Además de los resultados funcionales, la revisión docente valora la correspondencia entre el problema, la intención del patrón y la estructura elegida. Una clase con el nombre del patrón no acredita el ejercicio.

## Pruebas académicas

Este desafío tiene tres casos independientes bajo el trait `ChallengeId=DP-11`. Se ejecutan sin servicios externos reales. Esperan el comportamiento final y fallan inicialmente al alcanzar el pendiente propio; no aprueban por obtener una excepción de funcionalidad pendiente.

Desde la raíz de AccountingLab:

```powershell
.\scripts\Invoke-Tests.ps1 -Category Academic -ChallengeId DP-11
```

El fallo inicial es evidencia del trabajo aún reservado al estudiante. No se deben alterar aserciones, omitir casos ni convertir el resultado fallido en una aprobación artificial.

## Preguntas conceptuales

- ¿Qué parte de un movimiento es intrínseca y compartible y qué parte pertenece exclusivamente a ese movimiento?
- ¿Qué evidencia funcional aportan las pruebas y qué decisiones de diseño necesitan revisión adicional?
- ¿Qué acoplamientos introduces y cómo explicarías sus ventajas y limitaciones?

## Referencia y política académica

[Flyweight en Refactoring.Guru](https://refactoring.guru/es/design-patterns/flyweight). La referencia ofrece contexto conceptual; no se incorporan sus soluciones al ejercicio.

La [política académica de AGENTS.md](../../../AGENTS.md) reserva la implementación al estudiante. Un asistente puede explicar conceptos, mensajes de error y pruebas, y formular preguntas; no puede completar este desafío ni entregar una solución equivalente.

