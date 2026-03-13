using AicaseCli.Core;
using AicaseCli.Models;

namespace AicaseCli.Generators;

/// <summary>
/// Genera todo el código C# .NET 8 Web API para el proyecto.
/// Utiliza plantillas Scriban y los modelos parseados del JSON.
/// </summary>
public class BackendGenerator
{
    private readonly TemplateEngine _engine;
    private readonly string _outputPath;

    public BackendGenerator(TemplateEngine engine, string outputPath)
    {
        _engine = engine;
        _outputPath = outputPath;
    }

    /// <summary>
    /// Genera todos los artefactos del backend para el proyecto dado.
    /// </summary>
    public async Task<int> GenerateAsync(ProjectDefinition project)
    {
        var backendPath = Path.Combine(_outputPath, "backend");
        int fileCount = 0;

        // Estructura base de carpetas
        var folders = new[] { "Models", "DTOs", "Data", "Repositories", "Services", "Controllers", "Validators" };
        foreach (var folder in folders)
            Directory.CreateDirectory(Path.Combine(backendPath, folder));

        // Generar archivos por entidad
        foreach (var entity in project.Entities)
        {
            var context = new { Project = project, Entity = entity };

            // Modelo
            fileCount += await RenderAndWrite(
                "backend/model.scriban", context,
                Path.Combine(backendPath, "Models", $"{entity.Name}.cs"));

            // DTOs
            fileCount += await RenderAndWrite(
                "backend/dto.scriban", context,
                Path.Combine(backendPath, "DTOs", $"{entity.Name}Dto.cs"));

            // Repositorio (interfaz + implementación)
            fileCount += await RenderAndWrite(
                "backend/repository-interface.scriban", context,
                Path.Combine(backendPath, "Repositories", $"I{entity.Name}Repository.cs"));

            fileCount += await RenderAndWrite(
                "backend/repository.scriban", context,
                Path.Combine(backendPath, "Repositories", $"{entity.Name}Repository.cs"));

            // Servicio (interfaz + implementación)
            fileCount += await RenderAndWrite(
                "backend/service-interface.scriban", context,
                Path.Combine(backendPath, "Services", $"I{entity.Name}Service.cs"));

            fileCount += await RenderAndWrite(
                "backend/service.scriban", context,
                Path.Combine(backendPath, "Services", $"{entity.Name}Service.cs"));

            // Controlador
            fileCount += await RenderAndWrite(
                "backend/controller.scriban", context,
                Path.Combine(backendPath, "Controllers", $"{entity.Name}sController.cs"));

            // Validador FluentValidation
            fileCount += await RenderAndWrite(
                "backend/validator.scriban", context,
                Path.Combine(backendPath, "Validators", $"{entity.Name}Validator.cs"));

            // DbContext configuration (Fluent API)
            fileCount += await RenderAndWrite(
                "backend/dbcontext-config.scriban", context,
                Path.Combine(backendPath, "Data", $"{entity.Name}Configuration.cs"));

            Console.WriteLine($"    → {entity.Name}: 9 archivos");
        }

        // DbContext principal
        var dbContextContext = new { Project = project };
        fileCount += await RenderAndWrite(
            "backend/dbcontext.scriban", dbContextContext,
            Path.Combine(backendPath, "Data", "ApplicationDbContext.cs"));

        // Program.cs y csproj
        fileCount += await RenderAndWrite(
            "backend/program.scriban", dbContextContext,
            Path.Combine(backendPath, "Program.cs"));

        fileCount += await RenderAndWrite(
            "backend/csproj.scriban", dbContextContext,
            Path.Combine(backendPath, $"{project.CSharpNamespace}.csproj"));

        // appsettings.json
        fileCount += await RenderAndWrite(
            "backend/appsettings.scriban", dbContextContext,
            Path.Combine(backendPath, "appsettings.json"));

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
