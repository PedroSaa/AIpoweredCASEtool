using NJsonSchema;

namespace AicaseCli.Core;

/// <summary>
/// Valida un JSON de definición de proyecto contra el esquema AICASE usando NJsonSchema.
/// </summary>
public class SchemaValidator
{
    /// <summary>
    /// Valida el contenido JSON contra el esquema especificado.
    /// </summary>
    /// <param name="jsonContent">Contenido JSON del proyecto a validar.</param>
    /// <param name="schemaPath">Ruta al archivo aicase-schema.json.</param>
    /// <returns>Tupla con IsValid y colección de errores de validación.</returns>
    public async Task<(bool IsValid, ICollection<string> Errors)> ValidateAsync(
        string jsonContent,
        string schemaPath)
    {
        if (string.IsNullOrWhiteSpace(jsonContent))
            return (false, new[] { "El contenido JSON está vacío." });

        if (!File.Exists(schemaPath))
            return (false, new[] { $"El archivo de esquema no existe: {schemaPath}" });

        try
        {
            var schemaJson = await File.ReadAllTextAsync(schemaPath);
            var schema = await JsonSchema.FromJsonAsync(schemaJson);

            var errors = schema.Validate(jsonContent);

            if (errors.Count == 0)
                return (true, Array.Empty<string>());

            var errorMessages = errors
                .Select(e => $"[{e.Path}] {e.Kind}: {e.ToString()}")
                .ToList();

            return (false, errorMessages);
        }
        catch (Exception ex)
        {
            return (false, new[] { $"Error al procesar el esquema: {ex.Message}" });
        }
    }
}
