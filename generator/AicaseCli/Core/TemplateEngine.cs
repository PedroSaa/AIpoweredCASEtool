using Scriban;
using Scriban.Runtime;

namespace AicaseCli.Core;

/// <summary>
/// Motor de plantillas basado en Scriban para renderizar código a partir de modelos.
/// </summary>
public class TemplateEngine
{
    /// <summary>
    /// Renderiza una plantilla Scriban desde un archivo con el modelo dado.
    /// </summary>
    /// <param name="templatePath">Ruta al archivo .scriban.</param>
    /// <param name="model">Modelo de datos para la plantilla.</param>
    /// <returns>Cadena de texto con el código renderizado.</returns>
    public async Task<string> RenderAsync(string templatePath, object model)
    {
        if (!File.Exists(templatePath))
            throw new FileNotFoundException($"No se encontró la plantilla: {templatePath}");

        var templateContent = await File.ReadAllTextAsync(templatePath);
        return await RenderFromStringAsync(templateContent, model);
    }

    /// <summary>
    /// Renderiza una plantilla Scriban desde una cadena con el modelo dado.
    /// </summary>
    public async Task<string> RenderFromStringAsync(string templateContent, object model)
    {
        var template = Template.Parse(templateContent);

        if (template.HasErrors)
        {
            var errors = string.Join("\n", template.Messages.Select(m => m.ToString()));
            throw new InvalidOperationException($"Error al parsear la plantilla Scriban:\n{errors}");
        }

        var scriptObject = new ScriptObject();
        scriptObject.Import(model, renamer: member => member.Name.ToLowerInvariant()
            .Replace("_", string.Empty));

        // Funciones auxiliares de tipo personalizadas
        scriptObject.Import("csharp_type", new Func<string, string>(CSharpType));
        scriptObject.Import("typescript_type", new Func<string, string>(TypeScriptType));
        scriptObject.Import("sql_type", new Func<string, int?, int?, int?, string>(SqlType));
        scriptObject.Import("to_camel_case", new Func<string, string>(ToCamelCase));
        scriptObject.Import("to_plural", new Func<string, string>(ToPlural));
        scriptObject.Import("to_kebab_case", new Func<string, string>(ToKebabCase));

        var context = new TemplateContext();
        context.PushGlobal(scriptObject);

        var result = await template.RenderAsync(context);
        return result;
    }

    /// <summary>
    /// Renderiza una plantilla y escribe el resultado en un archivo.
    /// </summary>
    public async Task RenderToFileAsync(string templatePath, object model, string outputPath)
    {
        var content = await RenderAsync(templatePath, model);
        await WriteFileAsync(outputPath, content);
    }

    /// <summary>
    /// Renderiza una cadena de plantilla y escribe el resultado en un archivo.
    /// </summary>
    public async Task RenderStringToFileAsync(string templateContent, object model, string outputPath)
    {
        var content = await RenderFromStringAsync(templateContent, model);
        await WriteFileAsync(outputPath, content);
    }

    private static async Task WriteFileAsync(string outputPath, string content)
    {
        var directory = Path.GetDirectoryName(outputPath);
        if (!string.IsNullOrEmpty(directory))
            Directory.CreateDirectory(directory);

        await File.WriteAllTextAsync(outputPath, content);
    }

    // ── Funciones de conversión de tipos ─────────────────────────────────────

    private static string CSharpType(string jsonType) => jsonType switch
    {
        "string" => "string",
        "int" => "int",
        "long" => "long",
        "decimal" => "decimal",
        "double" => "double",
        "float" => "float",
        "date" or "datetime" => "DateTime",
        "bool" => "bool",
        "guid" => "Guid",
        "byte[]" => "byte[]",
        _ => "string"
    };

    private static string TypeScriptType(string jsonType) => jsonType switch
    {
        "string" or "guid" => "string",
        "int" or "long" or "decimal" or "double" or "float" => "number",
        "date" or "datetime" => "Date",
        "bool" => "boolean",
        "byte[]" => "string",
        _ => "string"
    };

    private static string SqlType(string jsonType, int? largo, int? precision, int? escala) => jsonType switch
    {
        "string" => largo.HasValue ? $"NVARCHAR({largo})" : "NVARCHAR(MAX)",
        "int" => "INT",
        "long" => "BIGINT",
        "decimal" => $"DECIMAL({precision ?? 18},{escala ?? 2})",
        "double" => "FLOAT",
        "float" => "REAL",
        "date" => "DATE",
        "datetime" => "DATETIME2",
        "bool" => "BIT",
        "guid" => "UNIQUEIDENTIFIER",
        "byte[]" => "VARBINARY(MAX)",
        _ => "NVARCHAR(255)"
    };

    private static string ToCamelCase(string input)
    {
        if (string.IsNullOrEmpty(input)) return input;
        return char.ToLowerInvariant(input[0]) + input[1..];
    }

    private static string ToPlural(string input)
    {
        if (string.IsNullOrEmpty(input)) return input;
        if (input.EndsWith("s", StringComparison.OrdinalIgnoreCase)) return input;
        if (input.EndsWith("y", StringComparison.OrdinalIgnoreCase))
            return input[..^1] + "ies";
        return input + "s";
    }

    private static string ToKebabCase(string input)
    {
        if (string.IsNullOrEmpty(input)) return input;
        var result = System.Text.RegularExpressions.Regex.Replace(
            input,
            "(?<=[a-z0-9])(?=[A-Z])",
            "-");
        return result.ToLowerInvariant();
    }
}
