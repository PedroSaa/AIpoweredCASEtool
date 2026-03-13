using System.CommandLine;
using AicaseCli.Core;
using AicaseCli.Generators;

// ─── CLI Definition ─────────────────────────────────────────────────────────

var inputOption = new Option<FileInfo>(
    name: "--input",
    description: "Ruta al archivo JSON del proyecto")
{
    IsRequired = true
};
inputOption.AddAlias("-i");

var outputOption = new Option<DirectoryInfo>(
    name: "--output",
    description: "Carpeta de salida para el código generado",
    getDefaultValue: () => new DirectoryInfo("output"));
outputOption.AddAlias("-o");

var schemaOption = new Option<FileInfo?>(
    name: "--schema",
    description: "Ruta al JSON Schema de AICASE (opcional, usa el embebido por defecto)");
schemaOption.AddAlias("-s");

var rootCommand = new RootCommand("AICASE - Generador de código AI-powered CASE tool");
rootCommand.AddOption(inputOption);
rootCommand.AddOption(outputOption);
rootCommand.AddOption(schemaOption);

rootCommand.SetHandler(async (input, output, schema) =>
{
    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.WriteLine("╔══════════════════════════════════════════════════╗");
    Console.WriteLine("║  AICASE - AI-powered CASE Tool                  ║");
    Console.WriteLine("║  Artificial Intelligence Computer-Aided          ║");
    Console.WriteLine("║  Software Engineering                            ║");
    Console.WriteLine("╚══════════════════════════════════════════════════╝");
    Console.ResetColor();
    Console.WriteLine();

    try
    {
        // 1. Validar archivo de entrada
        if (!input.Exists)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"✗ Error: No se encontró el archivo de entrada: {input.FullName}");
            Console.ResetColor();
            Environment.Exit(1);
            return;
        }

        Console.WriteLine($"📄 Archivo de entrada: {input.FullName}");
        Console.WriteLine($"📁 Carpeta de salida:  {output.FullName}");
        Console.WriteLine();

        // 2. Validar JSON contra el schema
        Console.Write("🔍 Validando JSON contra el schema... ");
        var validator = new SchemaValidator();
        var schemaPath = schema?.FullName;
        var validationResult = await validator.ValidateAsync(input.FullName, schemaPath);

        if (!validationResult.IsValid)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("✗ INVÁLIDO");
            Console.WriteLine("\nErrores de validación:");
            foreach (var error in validationResult.Errors)
            {
                Console.WriteLine($"  • {error}");
            }
            Console.ResetColor();
            Environment.Exit(1);
            return;
        }

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("✓ OK");
        Console.ResetColor();

        // 3. Parsear el JSON en modelos internos
        Console.Write("📊 Parseando definición del proyecto... ");
        var parser = new ProjectParser();
        var project = await parser.ParseAsync(input.FullName);
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"✓ OK ({project.Entities.Count} entidades encontradas)");
        Console.ResetColor();

        // 4. Preparar carpeta de salida
        if (!output.Exists)
        {
            output.Create();
        }

        // 5. Orquestar generación
        var templateEngine = new TemplateEngine();
        int totalFiles = 0;

        // Backend (C#)
        Console.WriteLine("\n🔧 Generando backend (C# .NET 8)...");
        var backendGenerator = new BackendGenerator(templateEngine, output.FullName);
        int backendFiles = await backendGenerator.GenerateAsync(project);
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"  ✓ {backendFiles} archivos generados");
        Console.ResetColor();
        totalFiles += backendFiles;

        // Frontend (Angular)
        Console.WriteLine("🅰️  Generando frontend (Angular 17)...");
        var frontendGenerator = new FrontendGenerator(templateEngine, output.FullName);
        int frontendFiles = await frontendGenerator.GenerateAsync(project);
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"  ✓ {frontendFiles} archivos generados");
        Console.ResetColor();
        totalFiles += frontendFiles;

        // Base de datos (SQL)
        Console.WriteLine("🗄️  Generando scripts de base de datos (SQL Server)...");
        var databaseGenerator = new DatabaseGenerator(templateEngine, output.FullName);
        int dbFiles = await databaseGenerator.GenerateAsync(project);
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"  ✓ {dbFiles} archivos generados");
        Console.ResetColor();
        totalFiles += dbFiles;

        // 6. Resumen
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"✅ Generación completada: {totalFiles} archivos en '{output.FullName}'");
        Console.ResetColor();
        Console.WriteLine();
        Console.WriteLine("📋 Próximos pasos:");
        Console.WriteLine($"  1. cd {output.FullName}/backend");
        Console.WriteLine($"  2. dotnet restore && dotnet build");
        Console.WriteLine($"  3. cd {output.FullName}/frontend");
        Console.WriteLine($"  4. npm install && ng serve");
        Console.WriteLine($"  5. Ejecutar scripts SQL en: {output.FullName}/database/");
    }
    catch (Exception ex)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"\n✗ Error inesperado: {ex.Message}");
        Console.WriteLine(ex.StackTrace);
        Console.ResetColor();
        Environment.Exit(1);
    }
}, inputOption, outputOption, schemaOption);

return await rootCommand.InvokeAsync(args);
