using NJsonSchema;

namespace AicaseCli.Core;

/// <summary>
/// Valida un JSON de proyecto contra el schema formal de AICASE.
/// </summary>
public class SchemaValidator
{
    private const string DefaultSchemaResourceName = "schema/aicase-schema.json";

    /// <summary>
    /// Valida el JSON del archivo de entrada contra el schema AICASE.
    /// </summary>
    /// <param name="jsonFilePath">Ruta al archivo JSON del proyecto.</param>
    /// <param name="schemaFilePath">Ruta al schema JSON (null = usa el schema embebido).</param>
    public async Task<ValidationResult> ValidateAsync(string jsonFilePath, string? schemaFilePath = null)
    {
        string jsonContent = await File.ReadAllTextAsync(jsonFilePath);
        string schemaContent = await LoadSchemaAsync(schemaFilePath);

        var schema = await JsonSchema.FromJsonAsync(schemaContent);
        var errors = schema.Validate(jsonContent);

        return new ValidationResult
        {
            IsValid = errors.Count == 0,
            Errors = errors.Select(e => $"{e.Path}: {e.Kind}").ToList()
        };
    }

    private async Task<string> LoadSchemaAsync(string? schemaFilePath)
    {
        // Si se especificó un schema custom, usarlo
        if (schemaFilePath != null)
        {
            if (!File.Exists(schemaFilePath))
                throw new FileNotFoundException($"Schema no encontrado: {schemaFilePath}");
            return await File.ReadAllTextAsync(schemaFilePath);
        }

        // Buscar schema relativo al ejecutable o al directorio de trabajo
        var candidates = new[]
        {
            Path.Combine(AppContext.BaseDirectory, DefaultSchemaResourceName),
            Path.Combine(Directory.GetCurrentDirectory(), DefaultSchemaResourceName),
            Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", DefaultSchemaResourceName)
        };

        foreach (var candidate in candidates)
        {
            if (File.Exists(candidate))
                return await File.ReadAllTextAsync(candidate);
        }

        throw new FileNotFoundException(
            $"No se encontró el schema AICASE. Especifique la ruta con --schema o coloque el schema en: {DefaultSchemaResourceName}");
    }
}

/// <summary>
/// Resultado de validación del schema.
/// </summary>
public class ValidationResult
{
    public bool IsValid { get; init; }
    public List<string> Errors { get; init; } = [];
}
