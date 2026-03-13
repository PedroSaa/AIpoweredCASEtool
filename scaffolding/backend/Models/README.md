# Modelos

Esta carpeta contiene las clases de entidades (modelos de dominio) **generadas automáticamente por AICASE**.

Cada archivo corresponde a una entidad definida en el JSON del proyecto y está decorado con atributos de `System.ComponentModel.DataAnnotations` para validaciones y `System.ComponentModel.DataAnnotations.Schema` para el mapeo con la base de datos.

> ⚠️ **No modifique estos archivos manualmente.** Los cambios se perderán al regenerar. Si necesita extender una entidad, use clases parciales (`partial class`) en archivos separados.

## Archivos generados

- `{Entidad}.cs` — Clase de modelo para cada entidad definida en el JSON.
