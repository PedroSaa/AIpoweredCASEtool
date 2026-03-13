# Repositorios

Esta carpeta contiene las interfaces e implementaciones de repositorios **generadas automáticamente por AICASE**.

El patrón de repositorio abstrae el acceso a datos (Entity Framework Core) del resto de la aplicación. Por cada entidad se generan:

- `I{Entidad}Repository.cs` — Interfaz con los métodos de acceso a datos.
- `{Entidad}Repository.cs` — Implementación que usa `ApplicationDbContext`.

Los repositorios están registrados en el contenedor de inyección de dependencias en `Program.cs`.

## Métodos estándar generados

- `GetAllAsync(int page, int pageSize)` — Obtiene todos con paginación.
- `GetByIdAsync(int id)` — Obtiene por clave primaria.
- `CreateAsync(T entity)` — Inserta un nuevo registro.
- `UpdateAsync(T entity)` — Actualiza un registro existente.
- `DeleteAsync(int id)` — Elimina un registro.
- `ExistsAsync(int id)` — Verifica si existe un registro.

> ⚠️ **No modifique estos archivos manualmente.** Los cambios se perderán al regenerar.
