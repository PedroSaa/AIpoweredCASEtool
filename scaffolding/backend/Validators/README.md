# Validadores

Esta carpeta contiene los validadores de FluentValidation **generados automáticamente por AICASE**.

Por cada entidad se genera un validador para el DTO de creación/actualización, con las reglas definidas en la sección `validaciones` de cada campo en el JSON del proyecto.

- `{Entidad}CreateValidator.cs` — Valida el DTO de creación.
- `{Entidad}UpdateValidator.cs` — Hereda de CreateValidator e incluye validación del Id.

## Reglas generadas automáticamente

| Validación JSON  | Regla FluentValidation generada              |
|------------------|----------------------------------------------|
| `requerido`      | `.NotEmpty().NotNull()`                      |
| `minLength`      | `.MinimumLength(n)`                          |
| `maxLength`      | `.MaximumLength(n)`                          |
| `email`          | `.EmailAddress()`                            |
| `min`            | `.GreaterThanOrEqualTo(n)`                   |
| `max`            | `.LessThanOrEqualTo(n)`                      |
| `regex`          | `.Matches("pattern")`                        |
| `fechaMaxima`    | `.LessThanOrEqualTo(DateTime.Today)`         |
| `fechaMinima`    | `.GreaterThanOrEqualTo(minDate)`             |

Los validadores se registran automáticamente en `Program.cs` mediante `AddValidatorsFromAssemblyContaining<Program>()`.

> ⚠️ **No modifique estos archivos manualmente.** Los cambios se perderán al regenerar.
