# DP-06 — Adapter

## Problema y contexto empresarial

Importar extractos de BancoA y BancoB ficticios, cuyos JSON expresan identificaciones, fechas e importes con convenciones incompatibles.

## Situación inicial

El contrato existe y compila. Su cuerpo conserva el marcador `TODO(STUDENT): DP-06` y lanza un fallo académico identificado. El comportamiento completo queda reservado al estudiante; el soporte básico de la plataforma no lo proporciona.

## Contrato y punto de extensión

- Capa: Infrastructure.
- Archivo: [Challenge.cs](../../../src/AccountingLab.Infrastructure/Challenges/DP-06_ADAPTER/Challenge.cs).
- Contrato público: `BankImportChallenge.Import(BankImportRequest request)`.
- Pruebas: [ChallengeTests.cs](../../../tests/AccountingLab.PatternChallenges/DP-06_ADAPTER/ChallengeTests.cs).

Los tipos públicos de entrada y salida describen datos observables. No constituyen los participantes internos de una solución y no obligan a copiar una implementación externa. Las precondiciones de cada caso están en sus datos de prueba.

## Trabajo pendiente

El estudiante analiza el problema y desarrolla una solución propia que satisfaga el contrato y la intención de Adapter. Debe justificar responsabilidades, dependencias, extensibilidad y límites. Ninguna guía inicial contiene la colaboración resuelta ni pseudocódigo.

## Criterios de aceptación observables

BancoA usa id/date/description/debit/credit/currency; BancoB usa reference/bookedOn/concept/signedAmount/isoCurrency. Un signedAmount negativo representa débito; positivo representa crédito. Los importes se conservan exactamente y el JSON inválido se rechaza con FormatException.

Además de los resultados funcionales, la revisión docente valora la correspondencia entre el problema, la intención del patrón y la estructura elegida. Una clase con el nombre del patrón no acredita el ejercicio.

## Pruebas académicas

Este desafío tiene tres casos independientes bajo el trait `ChallengeId=DP-06`. Se ejecutan sin servicios externos reales. Esperan el comportamiento final y fallan inicialmente al alcanzar el pendiente propio; no aprueban por obtener una excepción de funcionalidad pendiente.

Desde la raíz de AccountingLab:

```powershell
.\scripts\Invoke-Tests.ps1 -Category Academic -ChallengeId DP-06
```

El fallo inicial es evidencia del trabajo aún reservado al estudiante. No se deben alterar aserciones, omitir casos ni convertir el resultado fallido en una aprobación artificial.

## Preguntas conceptuales

- ¿Qué incompatibilidad pertenece al proveedor y qué contrato debe ver el resto de la contabilidad?
- ¿Qué evidencia funcional aportan las pruebas y qué decisiones de diseño necesitan revisión adicional?
- ¿Qué acoplamientos introduces y cómo explicarías sus ventajas y limitaciones?

## Referencia y política académica

[Adapter en Refactoring.Guru](https://refactoring.guru/es/design-patterns/adapter). La referencia ofrece contexto conceptual; no se incorporan sus soluciones al ejercicio.

La [política académica de AGENTS.md](../../../AGENTS.md) reserva la implementación al estudiante. Un asistente puede explicar conceptos, mensajes de error y pruebas, y formular preguntas; no puede completar este desafío ni entregar una solución equivalente.

