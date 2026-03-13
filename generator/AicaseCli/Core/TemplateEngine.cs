using System.Reflection;
using Scriban;
using Scriban.Runtime;

namespace AicaseCli.Core;

/// <summary>
/// Motor de plantillas basado en Scriban.
/// Carga plantillas desde archivos .scriban y las renderiza con los datos del proyecto.
/// </summary>
public class TemplateEngine
{
    private readonly string _templatesBasePath;

    public TemplateEngine()
    {
        // Buscar carpeta templates relativa al ejecutable o al directorio de trabajo
        _templatesBasePath = FindTemplatesPath();
    }

    /// <summary>
    /// Renderiza una plantilla con el contexto dado.
    /// </summary>
    /// <param name="templateRelativePath">Ruta relativa de la plantilla (ej: "backend/model.scriban")</param>
    /// <param name="model">Objeto con los datos para la plantilla</param>
    public async Task<string> RenderAsync(string templateRelativePath, object model)
    {
        var templatePath = Path.Combine(_templatesBasePath, templateRelativePath);

        if (!File.Exists(templatePath))
            throw new FileNotFoundException($"Plantilla no encontrada: {templatePath}");

        var templateContent = await File.ReadAllTextAsync(templatePath);
        var template = Template.Parse(templateContent);

        if (template.HasErrors)
        {
            var errors = string.Join("\n", template.Messages.Select(m => m.Message));
            throw new InvalidOperationException($"Error en plantilla {templateRelativePath}:\n{errors}");
        }

        return await template.RenderAsync(CreateContext(model));
    }

    /// <summary>
    /// Renderiza una plantilla a partir de una cadena de texto (no de archivo).
    /// </summary>
    public async Task<string> RenderStringAsync(string templateContent, object model)
    {
        var template = Template.Parse(templateContent);

        if (template.HasErrors)
        {
            var errors = string.Join("\n", template.Messages.Select(m => m.Message));
            throw new InvalidOperationException($"Error en plantilla:\n{errors}");
        }

        return await template.RenderAsync(CreateContext(model));
    }

    private static TemplateContext CreateContext(object model)
    {
        var scriptObject = new ScriptObject();

        // Importar modelo con renamer que convierte todo a lowercase sin separadores
        scriptObject.Import(model, renamer: AllLowerRenamer);

        // Funciones helper disponibles en las plantillas
        scriptObject.Import("to_camel_case", new Func<string, string>(ToCamelCase));
        scriptObject.Import("to_pascal_case", new Func<string, string>(ToPascalCase));
        scriptObject.Import("to_kebab_case", new Func<string, string>(ToKebabCase));
        scriptObject.Import("to_snake_case", new Func<string, string>(ToSnakeCase));
        scriptObject.Import("pluralize", new Func<string, string>(Pluralize));

        var context = new TemplateContext
        {
            // Usar el mismo renamer para acceso a propiedades de objetos anidados
            MemberRenamer = AllLowerRenamer
        };
        context.PushGlobal(scriptObject);

        return context;
    }

    private static string FindTemplatesPath()
    {
        var candidates = new[]
        {
            Path.Combine(AppContext.BaseDirectory, "templates"),
            Path.Combine(Directory.GetCurrentDirectory(), "templates"),
            Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "templates")
        };

        foreach (var candidate in candidates)
        {
            if (Directory.Exists(candidate))
                return candidate;
        }

        // Si no se encuentra, retornar el primero (generará error descriptivo al intentar cargar)
        return candidates[0];
    }

    // ─── Renamer ─────────────────────────────────────────────────────────────

    /// <summary>Convierte el nombre del miembro a minúsculas (sin separadores).</summary>
    private static string AllLowerRenamer(MemberInfo member) =>
        member.Name.ToLowerInvariant();

    // ─── Funciones helper para Scriban ───────────────────────────────────────

    public static string ToCamelCase(string s)
    {
        if (string.IsNullOrEmpty(s)) return s;
        return char.ToLower(s[0]) + s[1..];
    }

    public static string ToPascalCase(string s)
    {
        if (string.IsNullOrEmpty(s)) return s;
        return char.ToUpper(s[0]) + s[1..];
    }

    public static string ToKebabCase(string s)
    {
        return string.Concat(s.Select((c, i) =>
            i > 0 && char.IsUpper(c) ? "-" + char.ToLower(c) : char.ToLower(c).ToString()));
    }

    public static string ToSnakeCase(string s)
    {
        return string.Concat(s.Select((c, i) =>
            i > 0 && char.IsUpper(c) ? "_" + char.ToLower(c) : char.ToLower(c).ToString()));
    }

    public static string Pluralize(string s)
    {
        if (string.IsNullOrEmpty(s)) return s;
        if (s.EndsWith("y", StringComparison.OrdinalIgnoreCase))
            return s[..^1] + "ies";
        if (s.EndsWith("s", StringComparison.OrdinalIgnoreCase))
            return s + "es";
        return s + "s";
    }
}
