using System.CommandLine;
using AicaseCli.Core;
using AicaseCli.Generators;

var inputOption = new Option<FileInfo?>(
    aliases: new[] { "--input", "-i" },
    description: "Ruta al archivo JSON de definición del proyecto (aicase-schema.json)")
{
    IsRequired = true
};

var outputOption = new Option<DirectoryInfo>(
    aliases: new[] { "--output", "-o" },
    description: "Directorio de salida para el código generado",
    getDefaultValue: () => new DirectoryInfo("output"));

var schemaOption = new Option<FileInfo?>(
    aliases: new[] { "--schema", "-s" },
    description: "Ruta al archivo del esquema JSON para validación (aicase-schema.json)",
    getDefaultValue: () => new FileInfo(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "schema", "aicase-schema.json")));

var templatesOption = new Option<DirectoryInfo>(
    aliases: new[] { "--templates", "-t" },
    description: "Directorio raíz de las plantillas Scriban",
    getDefaultValue: () => new DirectoryInfo(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "templates")));

var skipValidationOption = new Option<bool>(
    aliases: new[] { "--skip-validation" },
    description: "Omitir validación del esquema JSON",
    getDefaultValue: () => false);

var verboseOption = new Option<bool>(
    aliases: new[] { "--verbose", "-v" },
    description: "Mostrar información detallada del proceso",
    getDefaultValue: () => false);

var rootCommand = new RootCommand("AICASE CLI - Generador de código para proyectos full-stack");
rootCommand.AddOption(inputOption);
rootCommand.AddOption(outputOption);
rootCommand.AddOption(schemaOption);
rootCommand.AddOption(templatesOption);
rootCommand.AddOption(skipValidationOption);
rootCommand.AddOption(verboseOption);

rootCommand.SetHandler(async (inputFile, outputDir, schemaFile, templatesDir, skipValidation, verbose) =>
{
    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.WriteLine("╔════════════════════════════════════════╗");
    Console.WriteLine("║    AICASE - Generador de Código v1.0   ║");
    Console.WriteLine("╚════════════════════════════════════════╝");
    Console.ResetColor();
    Console.WriteLine();

    try
    {
        // ── Validar que el archivo de entrada existe ───────────────────────────
        if (inputFile == null || !inputFile.Exists)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"[ERROR] El archivo de entrada no existe: {inputFile?.FullName}");
            Console.ResetColor();
            Environment.Exit(1);
            return;
        }

        Console.WriteLine($"[1/6] Leyendo archivo de entrada: {inputFile.FullName}");
        var jsonContent = await File.ReadAllTextAsync(inputFile.FullName);

        // ── Validar contra el esquema JSON ─────────────────────────────────────
        if (!skipValidation)
        {
            Console.WriteLine("[2/6] Validando contra el esquema AICASE...");
            var validator = new SchemaValidator();

            var schemaPath = schemaFile?.FullName;
            if (string.IsNullOrEmpty(schemaPath) || !File.Exists(schemaPath))
            {
                // Buscar el esquema en ubicaciones conocidas
                var candidates = new[]
                {
                    Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "schema", "aicase-schema.json"),
                    Path.Combine(Directory.GetCurrentDirectory(), "schema", "aicase-schema.json"),
                    Path.Combine(Directory.GetCurrentDirectory(), "aicase-schema.json")
                };

                schemaPath = candidates.FirstOrDefault(File.Exists);
            }

            if (string.IsNullOrEmpty(schemaPath) || !File.Exists(schemaPath))
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("[AVISO] No se encontró el archivo de esquema. Omitiendo validación.");
                Console.ResetColor();
            }
            else
            {
                var (isValid, errors) = await validator.ValidateAsync(jsonContent, schemaPath);
                if (!isValid)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("[ERROR] El archivo JSON no es válido:");
                    foreach (var error in errors)
                        Console.WriteLine($"  - {error}");
                    Console.ResetColor();
                    Environment.Exit(2);
                    return;
                }
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("  ✓ JSON válido según el esquema AICASE.");
                Console.ResetColor();
            }
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("[2/6] Validación de esquema omitida (--skip-validation).");
            Console.ResetColor();
        }

        // ── Parsear el JSON en modelos C# ──────────────────────────────────────
        Console.WriteLine("[3/6] Parseando definición del proyecto...");
        var parser = new ProjectParser();
        var project = parser.Parse(jsonContent);

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"  ✓ Proyecto: {project.Nombre} v{project.Version}");
        Console.WriteLine($"  ✓ Entidades encontradas: {project.Entidades?.Count ?? 0}");
        Console.ResetColor();

        // ── Preparar directorio de salida ──────────────────────────────────────
        var outputPath = outputDir.FullName;
        Directory.CreateDirectory(outputPath);

        var templateEngine = new TemplateEngine();
        var templatesPath = templatesDir.FullName;

        if (!Directory.Exists(templatesPath))
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"[AVISO] Directorio de plantillas no encontrado: {templatesPath}");
            Console.WriteLine("  Los generadores usarán plantillas embebidas.");
            Console.ResetColor();
            templatesPath = string.Empty;
        }

        // ── Generar código Backend ─────────────────────────────────────────────
        Console.WriteLine("[4/6] Generando código Backend (.NET 8)...");
        var backendGenerator = new BackendGenerator(templateEngine, templatesPath);
        await backendGenerator.GenerateAsync(project, outputPath);

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"  ✓ Backend generado en: {Path.Combine(outputPath, "backend")}");
        Console.ResetColor();

        // ── Generar código Frontend ────────────────────────────────────────────
        Console.WriteLine("[5/6] Generando código Frontend (Angular 17)...");
        var frontendGenerator = new FrontendGenerator(templateEngine, templatesPath);
        await frontendGenerator.GenerateAsync(project, outputPath);

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"  ✓ Frontend generado en: {Path.Combine(outputPath, "frontend")}");
        Console.ResetColor();

        // ── Generar scripts de Base de Datos ──────────────────────────────────
        Console.WriteLine("[6/6] Generando scripts de Base de Datos (SQL Server)...");
        var dbGenerator = new DatabaseGenerator(templateEngine, templatesPath);
        await dbGenerator.GenerateAsync(project, outputPath);

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"  ✓ Scripts SQL generados en: {Path.Combine(outputPath, "database")}");
        Console.ResetColor();

        // ── Resumen final ──────────────────────────────────────────────────────
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("╔════════════════════════════════════════╗");
        Console.WriteLine("║         ¡Generación completada!        ║");
        Console.WriteLine("╚════════════════════════════════════════╝");
        Console.ResetColor();
        Console.WriteLine($"Archivos generados en: {Path.GetFullPath(outputPath)}");

        if (verbose)
        {
            Console.WriteLine();
            Console.WriteLine("Estructura generada:");
            PrintDirectoryTree(outputPath, "  ");
        }
    }
    catch (Exception ex)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"[ERROR FATAL] {ex.Message}");
        if (verbose)
            Console.WriteLine(ex.StackTrace);
        Console.ResetColor();
        Environment.Exit(99);
    }
}, inputOption, outputOption, schemaOption, templatesOption, skipValidationOption, verboseOption);

return await rootCommand.InvokeAsync(args);

static void PrintDirectoryTree(string path, string indent)
{
    if (!Directory.Exists(path)) return;

    foreach (var dir in Directory.GetDirectories(path))
    {
        Console.WriteLine($"{indent}📁 {Path.GetFileName(dir)}/");
        PrintDirectoryTree(dir, indent + "  ");
    }

    foreach (var file in Directory.GetFiles(path))
    {
        Console.WriteLine($"{indent}📄 {Path.GetFileName(file)}");
    }
}
