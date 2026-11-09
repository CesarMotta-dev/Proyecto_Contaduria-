# Datos iniciales ficticios

El inicializador de .NET, invocado exclusivamente mediante `--migrate-only`, es responsable de los datos de demostración. Debe proporcionar dos empresas y usuarios con roles que permitan comprobar aislamiento. Las contraseñas proceden de `Demo__Password`; los datos del laboratorio no representan personas, bancos ni normas fiscales reales.

El docente debe conservar la configuración local para volver a iniciar sesión. La contraseña no se escribe en los informes. Cambiarla en `.env` no autoriza sobrescribir credenciales o datos de una base ya utilizada: registra el comportamiento efectivo del inicializador en el informe.

Los avisos precargados se identifican como demostración. Las plantillas, extractos internos y demás datos iniciales no ejecutan clonación, importación bancaria, suscripciones ni otro desafío reservado al estudiante.

No se ejecutan patrones pendientes durante el arranque. La repetición del inicializador debe ser inocua sobre datos ya existentes; su comprobación se registra en [ENTREGA](../../docs/ENTREGA.md).
