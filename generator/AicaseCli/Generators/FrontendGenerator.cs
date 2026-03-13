using AicaseCli.Core;
using AicaseCli.Models;

namespace AicaseCli.Generators;

/// <summary>
/// Generates all Angular 19 frontend code using Scriban templates:
/// TypeScript interfaces, Angular services, list components, form components,
/// and routing module.
/// </summary>
public sealed class FrontendGenerator
{
    private readonly TemplateEngine _engine;
    private readonly string         _outputRoot;
    private readonly bool           _verbose;

    public FrontendGenerator(TemplateEngine engine, string outputRoot, bool verbose = false)
    {
        _engine     = engine;
        _outputRoot = outputRoot;
        _verbose    = verbose;
    }

    public async Task<List<string>> GenerateAsync(ProjectDefinition project)
    {
        var generated    = new List<string>();
        var frontendDir  = Path.Combine(_outputRoot, "frontend", "src", "app");

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("  [Frontend]");
        Console.ResetColor();

        foreach (var entity in project.Entities)
        {
            var kebab = TemplateEngine.ToKebabCase(entity.Name);
            var model = BuildModel(project, entity);

            // TypeScript interface
            generated.Add(await _engine.RenderToFileAsync(
                "frontend/model.scriban",
                model,
                Path.Combine(frontendDir, "models", $"{kebab}.model.ts")));

            // Angular service
            generated.Add(await _engine.RenderToFileAsync(
                "frontend/service.scriban",
                model,
                Path.Combine(frontendDir, "services", $"{kebab}.service.ts")));

            // List component TypeScript
            generated.Add(await _engine.RenderToFileAsync(
                "frontend/list-component-ts.scriban",
                model,
                Path.Combine(frontendDir, "components", kebab, $"{kebab}-list", $"{kebab}-list.component.ts")));

            // List component HTML
            generated.Add(await _engine.RenderToFileAsync(
                "frontend/list-component-html.scriban",
                model,
                Path.Combine(frontendDir, "components", kebab, $"{kebab}-list", $"{kebab}-list.component.html")));

            // Form component TypeScript
            generated.Add(await _engine.RenderToFileAsync(
                "frontend/form-component-ts.scriban",
                model,
                Path.Combine(frontendDir, "components", kebab, $"{kebab}-form", $"{kebab}-form.component.ts")));

            // Form component HTML
            generated.Add(await _engine.RenderToFileAsync(
                "frontend/form-component-html.scriban",
                model,
                Path.Combine(frontendDir, "components", kebab, $"{kebab}-form", $"{kebab}-form.component.html")));

            Console.WriteLine($"    {entity.Name}: 6 files");
        }

        // Routing module (one per project, includes all entities)
        var routingModel = new
        {
            Project     = project.Project,
            Entities    = project.Entities,
            AllEntities = project.Entities
        };

        generated.Add(await _engine.RenderToFileAsync(
            "frontend/routing-module.scriban",
            routingModel,
            Path.Combine(frontendDir, "app-routing.module.ts")));

        Console.WriteLine("    Routing module: 1 file");

        return generated;
    }

    private static object BuildModel(ProjectDefinition project, EntityDefinition entity)
    {
        return new
        {
            Entity      = entity,
            Project     = project.Project,
            GlobalConfig = project.GlobalConfig,
            AllEntities = project.Entities,
            KebabName   = TemplateEngine.ToKebabCase(entity.Name)
        };
    }
}
