using AicaseCli.Core;
using AicaseCli.Models;

namespace AicaseCli.Generators;

/// <summary>
/// Genera todo el código Angular 19 para el proyecto.
/// </summary>
public class FrontendGenerator
{
    private readonly TemplateEngine _engine;
    private readonly string _outputPath;

    public FrontendGenerator(TemplateEngine engine, string outputPath)
    {
        _engine = engine;
        _outputPath = outputPath;
    }

    /// <summary>
    /// Genera todos los artefactos del frontend para el proyecto dado.
    /// </summary>
    public async Task<int> GenerateAsync(ProjectDefinition project)
    {
        var frontendPath = Path.Combine(_outputPath, "frontend", "src", "app");
        int fileCount = 0;

        Directory.CreateDirectory(Path.Combine(frontendPath, "models"));
        Directory.CreateDirectory(Path.Combine(frontendPath, "services"));
        Directory.CreateDirectory(Path.Combine(frontendPath, "guards"));
        Directory.CreateDirectory(Path.Combine(frontendPath, "interceptors"));

        // Generar archivos por entidad
        foreach (var entity in project.Entities)
        {
            var entityFolderName = entity.AngularFileName;
            var componentFolder = Path.Combine(frontendPath, "components", entityFolderName);
            var listFolder = Path.Combine(componentFolder, $"{entityFolderName}-list");
            var formFolder = Path.Combine(componentFolder, $"{entityFolderName}-form");

            Directory.CreateDirectory(listFolder);
            Directory.CreateDirectory(formFolder);

            var context = new { Project = project, Entity = entity };

            // Modelo TypeScript
            fileCount += await RenderAndWrite(
                "frontend/model.scriban", context,
                Path.Combine(frontendPath, "models", $"{entityFolderName}.model.ts"));

            // Servicio Angular
            fileCount += await RenderAndWrite(
                "frontend/service.scriban", context,
                Path.Combine(frontendPath, "services", $"{entityFolderName}.service.ts"));

            // Componente listado (.ts + .html)
            fileCount += await RenderAndWrite(
                "frontend/list-component-ts.scriban", context,
                Path.Combine(listFolder, $"{entityFolderName}-list.component.ts"));

            fileCount += await RenderAndWrite(
                "frontend/list-component-html.scriban", context,
                Path.Combine(listFolder, $"{entityFolderName}-list.component.html"));

            // Componente formulario (.ts + .html)
            fileCount += await RenderAndWrite(
                "frontend/form-component-ts.scriban", context,
                Path.Combine(formFolder, $"{entityFolderName}-form.component.ts"));

            fileCount += await RenderAndWrite(
                "frontend/form-component-html.scriban", context,
                Path.Combine(formFolder, $"{entityFolderName}-form.component.html"));

            // Routing module
            fileCount += await RenderAndWrite(
                "frontend/routing-module.scriban", context,
                Path.Combine(componentFolder, $"{entityFolderName}-routing.module.ts"));

            Console.WriteLine($"    → {entity.Name}: 7 archivos");
        }

        // app-routing principal
        var projectContext = new { Project = project };
        fileCount += await RenderAndWrite(
            "frontend/app-routing.scriban", projectContext,
            Path.Combine(frontendPath, "app-routing.module.ts"));

        return fileCount;
    }

    private async Task<int> RenderAndWrite(string templatePath, object context, string outputFile)
    {
        try
        {
            var content = await _engine.RenderAsync(templatePath, context);
            await File.WriteAllTextAsync(outputFile, content);
            return 1;
        }
        catch (FileNotFoundException)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"    ⚠ Plantilla no encontrada: {templatePath} (omitido)");
            Console.ResetColor();
            return 0;
        }
        catch (InvalidOperationException ex)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"    ⚠ Error en plantilla {templatePath}: {ex.Message.Split('\n')[0]} (omitido)");
            Console.ResetColor();
            return 0;
        }
    }
}
