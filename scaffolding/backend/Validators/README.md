# Validadores

Esta carpeta contiene los validadores FluentValidation generados por AICASE.

Cada entidad genera:
- `{Entidad}CreateValidator.cs` - Validaciones para crear
- `{Entidad}UpdateValidator.cs` - Validaciones para actualizar

Las validaciones reflejan las reglas definidas en el campo `validaciones` del JSON maestro.
