# Controladores

Esta carpeta contiene los controladores de la API REST **generados automáticamente por AICASE**.

Cada controlador expone los endpoints CRUD definidos en la propiedad `api` del JSON del proyecto. Por cada entidad se genera:

- `{Entidad}Controller.cs` — Controlador con rutas, autenticación y autorización.

## Endpoints estándar generados

| Método HTTP | Ruta                  | Descripción                       |
|-------------|------------------------|-----------------------------------|
| `GET`       | `/api/{entidad}`       | Obtiene lista paginada.           |
| `GET`       | `/api/{entidad}/{id}`  | Obtiene por Id.                   |
| `POST`      | `/api/{entidad}`       | Crea un nuevo registro.           |
| `PUT`       | `/api/{entidad}/{id}`  | Actualiza un registro existente.  |
| `DELETE`    | `/api/{entidad}/{id}`  | Elimina un registro.              |

Los controladores decorados con `[Authorize(Roles = "...")]` requieren autenticación JWT.

> ⚠️ **No modifique estos archivos manualmente.** Los cambios se perderán al regenerar.
