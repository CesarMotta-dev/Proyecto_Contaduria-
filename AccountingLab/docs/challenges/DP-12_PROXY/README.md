# DP-12 — Proxy

## Problema y contexto empresarial

Acceder a un reporte confidencial de una empresa mediante un contrato que preserve permisos y aislamiento.

## Situación inicial

El contrato existe y compila. Su cuerpo conserva el marcador `TODO(STUDENT): DP-12` y lanza un fallo académico identificado. El comportamiento completo queda reservado al estudiante; el soporte básico de la plataforma no lo proporciona.

## Contrato y punto de extensión

- Capa: Application.
- Archivo: [Challenge.cs](../../../src/AccountingLab.Application/Challenges/DP-12_PROXY/Challenge.cs).
- Contrato público: `SensitiveReportChallenge.Get(SensitiveReportRequest request, Func<SensitiveFinancialReport> loadReport)`.
- Pruebas: [ChallengeTests.cs](../../../tests/AccountingLab.PatternChallenges/DP-12_PROXY/ChallengeTests.cs).

Los tipos públicos de entrada y salida describen datos observables. No constituyen los participantes internos de una solución y no obligan a copiar una implementación externa. Las precondiciones de cada caso están en sus datos de prueba.

## Trabajo pendiente

El estudiante analiza el problema y desarrolla una solución propia que satisfaga el contrato y la intención de Proxy. Debe justificar responsabilidades, dependencias, extensibilidad y límites. Ninguna guía inicial contiene la colaboración resuelta ni pseudocódigo.

## Criterios de aceptación observables

Una consulta autorizada obtiene el reporte de su empresa; una consulta denegada no accede a la fuente y falla con UnauthorizedAccessException; un resultado de otra empresa se rechaza con InvalidOperationException.

Además de los resultados funcionales, la revisión docente valora la correspondencia entre el problema, la intención del patrón y la estructura elegida. Una clase con el nombre del patrón no acredita el ejercicio.

## Pruebas académicas

Este desafío tiene tres casos independientes bajo el trait `ChallengeId=DP-12`. Se ejecutan sin servicios externos reales. Esperan el comportamiento final y fallan inicialmente al alcanzar el pendiente propio; no aprueban por obtener una excepción de funcionalidad pendiente.

Desde la raíz de AccountingLab:

```powershell
.\scripts\Invoke-Tests.ps1 -Category Academic -ChallengeId DP-12
```

El fallo inicial es evidencia del trabajo aún reservado al estudiante. No se deben alterar aserciones, omitir casos ni convertir el resultado fallido en una aprobación artificial.

## Preguntas conceptuales

- ¿Por qué la autorización del servidor sigue siendo necesaria aunque exista un intermediario de acceso?
- ¿Qué evidencia funcional aportan las pruebas y qué decisiones de diseño necesitan revisión adicional?
- ¿Qué acoplamientos introduces y cómo explicarías sus ventajas y limitaciones?

## Referencia y política académica

[Proxy en Refactoring.Guru](https://refactoring.guru/es/design-patterns/proxy). La referencia ofrece contexto conceptual; no se incorporan sus soluciones al ejercicio.

La [política académica de AGENTS.md](../../../AGENTS.md) reserva la implementación al estudiante. Un asistente puede explicar conceptos, mensajes de error y pruebas, y formular preguntas; no puede completar este desafío ni entregar una solución equivalente.

