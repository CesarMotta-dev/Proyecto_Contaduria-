# AccountingLab · Web

Interfaz React + TypeScript independiente de la API. Requiere Node 22.12 o posterior (validado con 22.14).

```powershell
npm ci
npm run dev
```

Vite sirve la web en `http://localhost:5173` y reenvía `/api` a `http://localhost:5208`. Para otro servidor, copia `.env.example` a `.env.local` y define `VITE_API_URL` con la raíz completa de la API, incluido `/api`. El servidor remoto debe autorizar el origen de la web.

```powershell
npm test
npm run build
npm run preview
```

`npm test` ejecuta comprobaciones de interfaz y transporte HTTP y conserva un reporte JUnit en `output/tests/frontend.junit.xml`. No acredita la solución de los desafíos académicos. Las pruebas pueden simular respuestas HTTP; la aplicación de ejecución no tiene datos de respaldo simulados. La API debe estar disponible para las funciones operativas.

Las credenciales se configuran en el entorno del backend. No existe contraseña incorporada al cliente. El token y la empresa activa se conservan en la sesión de la pestaña; cerrar sesión los elimina. El avance autodeclarado se guarda localmente, por empresa, y aparece siempre separado de evidencia de pruebas.

Todos los importes se mantienen como cadenas decimales, y los cálculos oficiales se consultan al servidor. La web nunca resuelve un patrón para reemplazar una operación pendiente. Un HTTP 501 presenta ID, nombre y referencia del desafío sin mostrar un éxito contable.

La imagen Docker se construye con contexto `frontend/`, sirve el resultado mediante nginx y reenvía `/api` al servicio Compose `api:8080`.
