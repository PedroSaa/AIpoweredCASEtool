using AicaseCli.Core;
using AicaseCli.Models;

namespace AicaseCli.Generators;

/// <summary>
/// Generates SQL Server database scripts using Scriban templates:
/// CREATE TABLE statements with types, constraints and indexes,
/// and CRUD stored procedures for each entity.
/// </summary>
public sealed class DatabaseGenerator
{
    private readonly TemplateEngine _engine;
    private readonly string         _outputRoot;
    private readonly bool           _verbose;

    public DatabaseGenerator(TemplateEngine engine, string outputRoot, bool verbose = false)
    {
        _engine     = engine;
        _outputRoot = outputRoot;
        _verbose    = verbose;
    }

    public async Task<List<string>> GenerateAsync(ProjectDefinition project)
    {
        var generated  = new List<string>();
        var dbDir      = Path.Combine(_outputRoot, "database");
        var tablesDir  = Path.Combine(dbDir, "tables");
        var procsDir   = Path.Combine(dbDir, "stored-procedures");

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("  [Database]");
        Console.ResetColor();

        foreach (var entity in project.Entities)
        {
            var model = BuildModel(project, entity);

            // CREATE TABLE
            generated.Add(await _engine.RenderToFileAsync(
                "database/create-table.scriban",
                model,
                Path.Combine(tablesDir, $"Create_{entity.Table}.sql")));

            // CRUD stored procedures
            generated.Add(await _engine.RenderToFileAsync(
                "database/stored-procedure-crud.scriban",
                model,
                Path.Combine(procsDir, $"sp_{entity.Name}_CRUD.sql")));

            Console.WriteLine($"    {entity.Name}: 2 files");
        }

        // Master migration script that runs all tables + procs in order
        var masterModel = new
        {
            Project     = project.Project,
            Entities    = project.Entities,
            GlobalConfig = project.GlobalConfig,
            GeneratedAt = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")
        };

        var masterScript = BuildMasterScript(project);
        var masterPath   = Path.Combine(dbDir, "00_master_migration.sql");
        Directory.CreateDirectory(dbDir);
        await File.WriteAllTextAsync(masterPath, masterScript);
        generated.Add(masterPath);

        Console.WriteLine("    Master migration script: 1 file");

        return generated;
    }

    private static object BuildModel(ProjectDefinition project, EntityDefinition entity)
    {
        return new
        {
            Entity       = entity,
            Project      = project.Project,
            GlobalConfig = project.GlobalConfig,
            Schema       = project.GlobalConfig.Database.Schema,
            AllEntities  = project.Entities
        };
    }

    private static string BuildMasterScript(ProjectDefinition project)
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine($"-- ============================================================");
        sb.AppendLine($"-- AICASE Generated Migration Script");
        sb.AppendLine($"-- Project: {project.Project.Name} v{project.Project.Version}");
        sb.AppendLine($"-- Generated: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
        sb.AppendLine($"-- ============================================================");
        sb.AppendLine();
        sb.AppendLine($"USE [{project.Project.Name}];");
        sb.AppendLine("GO");
        sb.AppendLine();
        sb.AppendLine("-- ── Tables ──────────────────────────────────────────────────");
        foreach (var e in project.Entities)
            sb.AppendLine($":r .\\tables\\Create_{e.Table}.sql");
        sb.AppendLine();
        sb.AppendLine("-- ── Stored Procedures ───────────────────────────────────────");
        foreach (var e in project.Entities)
            sb.AppendLine($":r .\\stored-procedures\\sp_{e.Name}_CRUD.sql");
        sb.AppendLine();
        sb.AppendLine("PRINT 'Migration completed successfully.';");
        sb.AppendLine("GO");
        return sb.ToString();
    }
}
