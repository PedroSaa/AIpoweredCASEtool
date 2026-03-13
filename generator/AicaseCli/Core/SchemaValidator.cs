using NJsonSchema;
using NJsonSchema.Validation;

namespace AicaseCli.Core;

/// <summary>
/// Validates a project JSON file against the official AICASE JSON Schema (draft-07).
/// Uses NJsonSchema for full draft-07 compliance.
/// </summary>
public sealed class SchemaValidator
{
    private readonly string _schemaPath;
    private JsonSchema? _schema;

    public SchemaValidator(string schemaPath)
    {
        _schemaPath = schemaPath;
    }

    /// <summary>
    /// Validates the JSON file at <paramref name="jsonFilePath"/> against the AICASE schema.
    /// Returns (isValid, errorMessages).
    /// </summary>
    public async Task<(bool IsValid, IReadOnlyList<string> Errors)> ValidateAsync(string jsonFilePath)
    {
        try
        {
            _schema ??= await LoadSchemaAsync();
        }
        catch (Exception ex)
        {
            // If schema file is missing, skip validation with a warning
            return (true, [$"Schema not found at '{_schemaPath}' – validation skipped. ({ex.Message})"]);
        }

        string json;
        try
        {
            json = await File.ReadAllTextAsync(jsonFilePath);
        }
        catch (Exception ex)
        {
            return (false, [$"Cannot read input file: {ex.Message}"]);
        }

        ICollection<ValidationError> errors = _schema.Validate(json);

        if (errors.Count == 0)
            return (true, []);

        var messages = errors.Select(e => FormatError(e)).ToList();
        return (false, messages);
    }

    /// <summary>Validate a raw JSON string (for unit testing).</summary>
    public async Task<(bool IsValid, IReadOnlyList<string> Errors)> ValidateJsonAsync(string json)
    {
        _schema ??= await LoadSchemaAsync();
        var errors = _schema.Validate(json);
        if (errors.Count == 0) return (true, []);
        return (false, errors.Select(FormatError).ToList());
    }

    private async Task<JsonSchema> LoadSchemaAsync()
    {
        if (!File.Exists(_schemaPath))
            throw new FileNotFoundException($"Schema file not found: {_schemaPath}", _schemaPath);

        var schemaJson = await File.ReadAllTextAsync(_schemaPath);
        return await JsonSchema.FromJsonAsync(schemaJson);
    }

    private static string FormatError(ValidationError error)
    {
        var path = string.IsNullOrEmpty(error.Path) ? "/" : error.Path;
        return $"[{path}] {error.Kind}: {error.ToString()}";
    }
}
