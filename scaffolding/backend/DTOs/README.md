# DTOs (Data Transfer Objects)

Esta carpeta contiene los objetos de transferencia de datos **generados automáticamente por AICASE**.

Los DTOs separan la capa de presentación del modelo de dominio. Por cada entidad se generan:

- `{Entidad}CreateDto.cs` — Datos requeridos para crear un registro.
- `{Entidad}UpdateDto.cs` — Datos para actualizar un registro (hereda de CreateDto e incluye Id).
- `{Entidad}ResponseDto.cs` — Datos devueltos por la API al cliente.

Los DTOs se mapean hacia/desde los modelos mediante **AutoMapper**. El perfil de mapeo se encuentra en la carpeta `Mappings/`.

> ⚠️ **No modifique estos archivos manualmente.** Los cambios se perderán al regenerar.
