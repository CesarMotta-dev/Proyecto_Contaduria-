# Migraciones de MySQL

El origen único del esquema son las migraciones EF Core de `src/AccountingLab.Infrastructure/`. Este directorio documenta su aplicación; no contiene un segundo esquema SQL mantenido en paralelo.

`InitialAccounting.sql` es una exportación generada e idempotente de esas migraciones para inspección. Los archivos C# de migración y su snapshot son la fuente del esquema. La migración inicial se identifica como `20261023054030_InitialAccounting`.

Compose ejecuta el servicio `migrate` con `dotnet AccountingLab.Api.dll --migrate-only`. El proceso espera la base saludable, aplica las migraciones y los datos iniciales y finaliza. La API depende de su salida correcta. El proceso HTTP normal no elimina bases ni vuelve a crear el esquema.

En ejecución directa, configura `ConnectionStrings__AccountingLab`, `Demo__Email` y `Demo__Password` y ejecuta:

```powershell
dotnet run --project src/AccountingLab.Api -- --migrate-only
```

Ejecuta una sola instancia de la migración para una misma base. Conserva la salida original y registra en [ENTREGA](../../docs/ENTREGA.md) las migraciones aplicadas y los comandos comprobados. La repetición debe conservar los datos existentes sin duplicar los datos ficticios. Las pruebas usan una base separada, nunca la base de demostración.

La existencia de esta guía no acredita que una migración se haya ejecutado. Ese resultado requiere su informe real.
