# Servicios

Esta carpeta contiene las interfaces e implementaciones de la capa de servicios (lógica de negocio) **generadas automáticamente por AICASE**.

La capa de servicios orquesta la lógica de negocio y coordina los repositorios, validaciones y mapeos. Por cada entidad se generan:

- `I{Entidad}Service.cs` — Interfaz del servicio.
- `{Entidad}Service.cs` — Implementación con stubs de reglas de negocio.

## Estructura de un servicio

Cada servicio implementa:

- `GetAllAsync(int page, int pageSize)` — Obtiene todos (paginado).
- `GetByIdAsync(int id)` — Obtiene por Id.
- `CreateAsync({Entidad}CreateDto dto)` — Crea y aplica reglas `antesDeInsertar`.
- `UpdateAsync(int id, {Entidad}UpdateDto dto)` — Actualiza y aplica reglas `antesDeActualizar`.
- `DeleteAsync(int id)` — Elimina y aplica reglas `antesDeEliminar`.

Las reglas de negocio definidas en el JSON aparecen como métodos protegidos con comentarios `TODO` para su implementación.

> ⚠️ **No modifique estos archivos manualmente.** Los cambios se perderán al regenerar. Implemente la lógica en clases parciales o servicios derivados.
