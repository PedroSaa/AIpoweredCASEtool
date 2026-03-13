using AicaseCli.Core;
using AicaseCli.Models;

namespace AicaseCli.Generators;

/// <summary>
/// Genera scripts SQL para SQL Server: CREATE TABLE y Stored Procedures.
/// </summary>
public class DatabaseGenerator
{
    private readonly TemplateEngine _engine;
    private readonly string _outputPath;

    public DatabaseGenerator(TemplateEngine engine, string outputPath)
    {
        _engine = engine;
        _outputPath = outputPath;
    }

    /// <summary>
    /// Genera todos los scripts SQL para el proyecto.
    /// </summary>
    public async Task<int> GenerateAsync(ProjectDefinition project)
    {
        var dbPath = Path.Combine(_outputPath, "database");
        var spPath = Path.Combine(dbPath, "stored-procedures");
        Directory.CreateDirectory(dbPath);
        Directory.CreateDirectory(spPath);

        int fileCount = 0;

        // Generar CREATE TABLE por entidad
        foreach (var entity in project.Entities)
        {
            var context = new { Project = project, Entity = entity };

            // Script CREATE TABLE
            fileCount += await RenderAndWrite(
                "database/create-table.scriban", context,
                Path.Combine(dbPath, $"Create_{entity.TableName}.sql"));

            // Stored procedures (si está configurado)
            if (project.GlobalConfig.DatabaseConfig.UseStoredProcedures)
            {
                fileCount += await RenderAndWrite(
                    "database/stored-procedure-crud.scriban", context,
                    Path.Combine(spPath, $"SP_{entity.TableName}_CRUD.sql"));
            }

            Console.WriteLine($"    → {entity.Name}: {(project.GlobalConfig.DatabaseConfig.UseStoredProcedures ? 2 : 1)} archivo(s)");
        }

        // Script maestro que ejecuta todo en orden
        var masterScript = GenerateMasterScript(project, dbPath);
        await File.WriteAllTextAsync(Path.Combine(dbPath, "00_CreateDatabase.sql"), masterScript);
        fileCount++;

        return fileCount;
    }

    private static string GenerateMasterScript(ProjectDefinition project, string dbPath)
    {
        var lines = new List<string>
        {
            $"-- ============================================================",
            $"-- Script maestro: {project.Name}",
            $"-- Generado por AICASE v{project.Version}",
            $"-- ============================================================",
            "",
            $"USE master;",
            $"GO",
            "",
            $"IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'{project.GlobalConfig.DatabaseConfig.DatabaseName}')",
            $"BEGIN",
            $"    CREATE DATABASE [{project.GlobalConfig.DatabaseConfig.DatabaseName}];",
            $"END",
            $"GO",
            "",
            $"USE [{project.GlobalConfig.DatabaseConfig.DatabaseName}];",
            $"GO",
            "",
            "-- Ejecutar scripts de tablas en orden (respetando FKs):",
        };

        foreach (var entity in project.Entities)
        {
            lines.Add($"-- :r .\\Create_{entity.TableName}.sql");
        }

        lines.Add("");
        lines.Add("-- Fin del script maestro");

        return string.Join("\n", lines);
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
