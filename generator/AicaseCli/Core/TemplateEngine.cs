using Scriban;
using Scriban.Runtime;

namespace AicaseCli.Core;

/// <summary>
/// Scriban-based template engine.
/// Loads .scriban files from the configured templates directory,
/// renders them with the provided model data, and writes the output.
/// </summary>
public sealed class TemplateEngine
{
    private readonly string _templatesDir;
    private readonly bool   _verbose;
    private readonly Dictionary<string, Template> _cache = new();

    public TemplateEngine(string templatesDir, bool verbose = false)
    {
        _templatesDir = templatesDir;
        _verbose      = verbose;
    }

    /// <summary>
    /// Render a template and write to <paramref name="outputPath"/>.
    /// Returns <paramref name="outputPath"/> for caller to track generated files.
    /// </summary>
    public async Task<string> RenderToFileAsync(
        string      templateRelativePath,
        object      model,
        string      outputPath)
    {
        var rendered = await RenderAsync(templateRelativePath, model);
        var dir = Path.GetDirectoryName(outputPath)!;
        Directory.CreateDirectory(dir);
        await File.WriteAllTextAsync(outputPath, rendered);

        if (_verbose)
            Console.WriteLine($"    wrote: {outputPath}");

        return outputPath;
    }

    /// <summary>Render a template and return the string result.</summary>
    public async Task<string> RenderAsync(string templateRelativePath, object model)
    {
        var template = await GetOrLoadTemplateAsync(templateRelativePath);

        var scriptObject = new ScriptObject();
        scriptObject.Import(model, renamer: Scriban.Runtime.StandardMemberRenamer.Default);

        // Add helper functions
        AddHelpers(scriptObject);

        var context = new TemplateContext { StrictVariables = false };
        context.PushGlobal(scriptObject);

        return template.Render(context);
    }

    // ─── Template loading ─────────────────────────────────────────────────
    private async Task<Template> GetOrLoadTemplateAsync(string relativePath)
    {
        if (_cache.TryGetValue(relativePath, out var cached))
            return cached;

        var fullPath = Path.Combine(_templatesDir, relativePath);
        if (!File.Exists(fullPath))
            throw new FileNotFoundException($"Template not found: {fullPath}", fullPath);

        var source   = await File.ReadAllTextAsync(fullPath);
        var template = Template.Parse(source, fullPath);

        if (template.HasErrors)
        {
            var errors = string.Join("\n", template.Messages.Select(m => m.ToString()));
            throw new InvalidOperationException(
                $"Scriban parse errors in '{fullPath}':\n{errors}");
        }

        _cache[relativePath] = template;
        return template;
    }

    // ─── Helper functions exposed to templates ────────────────────────────
    private static void AddHelpers(ScriptObject obj)
    {
        // string helpers
        obj.SetValue("pascal_case",  new Func<string, string>(ToPascalCase),      true);
        obj.SetValue("camel_case",   new Func<string, string>(ToCamelCase),       true);
        obj.SetValue("kebab_case",   new Func<string, string>(ToKebabCase),       true);
        obj.SetValue("snake_case",   new Func<string, string>(ToSnakeCase),       true);
        obj.SetValue("plural",       new Func<string, string>(Pluralize),         true);

        // type mapping helpers
        obj.SetValue("csharp_type",     new Func<string, bool, string>(ToCSharpType),      true);
        obj.SetValue("typescript_type", new Func<string, string>(ToTypeScriptType),         true);
        obj.SetValue("sql_type",        new Func<string, int?, int?, int?, string>(ToSqlType), true);

        // misc
        obj.SetValue("join",  new Func<IEnumerable<string>, string, string>((items, sep) =>
            string.Join(sep, items)), true);
    }

    // ─── String transformations ───────────────────────────────────────────
    public static string ToPascalCase(string s)
    {
        if (string.IsNullOrEmpty(s)) return s;
        return char.ToUpperInvariant(s[0]) + s[1..];
    }

    public static string ToCamelCase(string s)
    {
        if (string.IsNullOrEmpty(s)) return s;
        return char.ToLowerInvariant(s[0]) + s[1..];
    }

    public static string ToKebabCase(string s)
    {
        if (string.IsNullOrEmpty(s)) return s;
        var result = new System.Text.StringBuilder();
        for (int i = 0; i < s.Length; i++)
        {
            if (char.IsUpper(s[i]) && i > 0)
                result.Append('-');
            result.Append(char.ToLowerInvariant(s[i]));
        }
        return result.ToString();
    }

    public static string ToSnakeCase(string s)
        => ToKebabCase(s).Replace('-', '_');

    public static string Pluralize(string s)
    {
        if (string.IsNullOrEmpty(s)) return s;
        if (s.EndsWith("s", StringComparison.OrdinalIgnoreCase)) return s + "es";
        if (s.EndsWith("y", StringComparison.OrdinalIgnoreCase)) return s[..^1] + "ies";
        return s + "s";
    }

    // ─── Type mappings ────────────────────────────────────────────────────
    public static string ToCSharpType(string fieldType, bool required)
    {
        var nullable = required ? "" : "?";
        return fieldType switch
        {
            "string"  => "string",               // reference type – always nullable in C#
            "int"     => $"int{nullable}",
            "decimal" => $"decimal{nullable}",
            "date"    => $"DateTime{nullable}",
            "bool"    => $"bool{nullable}",
            "guid"    => $"Guid{nullable}",
            _         => "object"
        };
    }

    public static string ToTypeScriptType(string fieldType) =>
        fieldType switch
        {
            "string"  => "string",
            "int"     => "number",
            "decimal" => "number",
            "date"    => "Date",
            "bool"    => "boolean",
            "guid"    => "string",
            _         => "unknown"
        };

    public static string ToSqlType(string fieldType, int? length, int? precision, int? scale) =>
        fieldType switch
        {
            "string"  => $"nvarchar({(length.HasValue ? length.Value.ToString() : "max")})",
            "int"     => "int",
            "decimal" => $"decimal({precision ?? 18}, {scale ?? 2})",
            "date"    => "datetime2",
            "bool"    => "bit",
            "guid"    => "uniqueidentifier",
            _         => "nvarchar(max)"
        };
}
