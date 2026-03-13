# Controladores

Esta carpeta contiene los controladores API generados por AICASE.

Cada entidad genera:
- `{Entidad}sController.cs` - API REST con endpoints CRUD

Los controladores incluyen:
- Atributos `[Authorize]` y `[Authorize(Roles="...")]`
- Documentación Swagger con `[ProducesResponseType]`
- Manejo de paginación en GET
