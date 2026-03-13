using AicaseCli.Core;
using AicaseCli.Models;

namespace AicaseCli.Generators;

/// <summary>
/// Generates all C# .NET 8 backend code using Scriban templates:
/// Models, DTOs, DbContext entity configs, Repository interfaces + implementations,
/// Service interfaces + implementations, Controllers, FluentValidation validators.
/// </summary>
public sealed class BackendGenerator
{
    private readonly TemplateEngine _engine;
    private readonly string         _outputRoot;
    private readonly bool           _verbose;

    public BackendGenerator(TemplateEngine engine, string outputRoot, bool verbose = false)
    {
        _engine     = engine;
        _outputRoot = outputRoot;
        _verbose    = verbose;
    }

    public async Task<List<string>> GenerateAsync(ProjectDefinition project)
    {
        var generated = new List<string>();
        var ns        = project.Project.Name;
        var backendDir = Path.Combine(_outputRoot, "backend");

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("  [Backend]");
        Console.ResetColor();

        foreach (var entity in project.Entities)
        {
            var model = BuildModel(project, entity, ns);

            // Model
            generated.Add(await _engine.RenderToFileAsync(
                "backend/model.scriban",
                model,
                Path.Combine(backendDir, "Models", $"{entity.Name}.cs")));

            // DTOs
            generated.Add(await _engine.RenderToFileAsync(
                "backend/dto.scriban",
                model,
                Path.Combine(backendDir, "DTOs", $"{entity.Name}Dto.cs")));

            // Repository interface
            generated.Add(await _engine.RenderToFileAsync(
                "backend/repository-interface.scriban",
                model,
                Path.Combine(backendDir, "Repositories", $"I{entity.Name}Repository.cs")));

            // Repository implementation
            generated.Add(await _engine.RenderToFileAsync(
                "backend/repository.scriban",
                model,
                Path.Combine(backendDir, "Repositories", $"{entity.Name}Repository.cs")));

            // Service interface
            generated.Add(await _engine.RenderToFileAsync(
                "backend/service-interface.scriban",
                model,
                Path.Combine(backendDir, "Services", $"I{entity.Name}Service.cs")));

            // Service implementation
            generated.Add(await _engine.RenderToFileAsync(
                "backend/service.scriban",
                model,
                Path.Combine(backendDir, "Services", $"{entity.Name}Service.cs")));

            // Controller
            generated.Add(await _engine.RenderToFileAsync(
                "backend/controller.scriban",
                model,
                Path.Combine(backendDir, "Controllers", $"{entity.Name}Controller.cs")));

            // Validator
            generated.Add(await _engine.RenderToFileAsync(
                "backend/validator.scriban",
                model,
                Path.Combine(backendDir, "Validators", $"{entity.Name}Validator.cs")));

            // EF Core entity configuration
            generated.Add(await _engine.RenderToFileAsync(
                "backend/dbcontext-config.scriban",
                model,
                Path.Combine(backendDir, "Data", "Configurations", $"{entity.Name}Configuration.cs")));

            Console.WriteLine($"    {entity.Name}: 9 files");
        }

        return generated;
    }

    private static object BuildModel(ProjectDefinition project, EntityDefinition entity, string ns)
    {
        return new
        {
            Namespace   = ns,
            Entity      = entity,
            Project     = project.Project,
            GlobalConfig = project.GlobalConfig,
            AllEntities = project.Entities
        };
    }
}
