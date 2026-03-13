using System.Text.Json;
using AicaseCli.Core;
using AicaseCli.Generators;
using AicaseCli.Models;

// ─── Banner ────────────────────────────────────────────────────────────────
Console.ForegroundColor = ConsoleColor.Cyan;
Console.WriteLine(@"
  █████╗ ██╗ ██████╗ █████╗ ███████╗███████╗
 ██╔══██╗██║██╔════╝██╔══██╗██╔════╝██╔════╝
 ███████║██║██║     ███████║███████╗█████╗
 ██╔══██║██║██║     ██╔══██║╚════██║██╔══╝
 ██║  ██║██║╚██████╗██║  ██║███████║███████╗
 ╚═╝  ╚═╝╚═╝ ╚═════╝╚═╝  ╚═╝╚══════╝╚══════╝
 AI-powered Computer-Aided Software Engineering
 Version 1.0.0
");
Console.ResetColor();

// ─── Argument parsing ─────────────────────────────────────────────────────
string? inputPath   = null;
string? outputPath  = null;
string? templateDir = null;
bool    verbose     = false;

for (int i = 0; i < args.Length; i++)
{
    switch (args[i])
    {
        case "--input"    or "-i": inputPath   = args[++i]; break;
        case "--output"   or "-o": outputPath  = args[++i]; break;
        case "--templates"or "-t": templateDir = args[++i]; break;
        case "--verbose"  or "-v": verbose = true;          break;
        case "--help"     or "-h":
            PrintHelp();
            return 0;
    }
}

if (inputPath is null || outputPath is null)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.Error.WriteLine("ERROR: --input and --output are required.");
    Console.ResetColor();
    PrintHelp();
    return 1;
}

if (!File.Exists(inputPath))
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.Error.WriteLine($"ERROR: Input file not found: {inputPath}");
    Console.ResetColor();
    return 1;
}

// Default templates directory: <exe-dir>/../../templates  (repo layout)
templateDir ??= Path.GetFullPath(
    Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "templates"));

if (!Directory.Exists(templateDir))
{
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.WriteLine($"WARNING: Templates directory not found at '{templateDir}'.");
    Console.WriteLine("         Set --templates to a valid path.");
    Console.ResetColor();
}

// ─── Step 1: Validate JSON schema ─────────────────────────────────────────
Console.WriteLine();
Console.Write("[ 1/4 ] Validating project JSON against schema... ");

var schemaPath = Path.GetFullPath(
    Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "schema", "aicase-schema.json"));

var validator = new SchemaValidator(schemaPath);
var (isValid, validationErrors) = await validator.ValidateAsync(inputPath);

if (!isValid)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine("FAILED");
    Console.WriteLine();
    foreach (var err in validationErrors)
        Console.Error.WriteLine($"  • {err}");
    Console.ResetColor();
    return 1;
}
Console.ForegroundColor = ConsoleColor.Green;
Console.WriteLine("OK");
Console.ResetColor();

// ─── Step 2: Parse JSON ───────────────────────────────────────────────────
Console.Write("[ 2/4 ] Parsing project definition... ");

var jsonText = await File.ReadAllTextAsync(inputPath);
ProjectDefinition project;
try
{
    project = ProjectParser.Parse(jsonText);
}
catch (Exception ex)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine("FAILED");
    Console.Error.WriteLine($"  Parse error: {ex.Message}");
    Console.ResetColor();
    return 1;
}

Console.ForegroundColor = ConsoleColor.Green;
Console.WriteLine($"OK  →  Project: {project.Project.Name} v{project.Project.Version} ({project.Entities.Count} entities)");
Console.ResetColor();

// ─── Step 3: Setup output directory ──────────────────────────────────────
Directory.CreateDirectory(outputPath);
var templateEngine = new TemplateEngine(templateDir, verbose);

// ─── Step 4: Generate code ────────────────────────────────────────────────
Console.WriteLine("[ 3/4 ] Generating code...");
Console.WriteLine();

var generatedFiles = new List<string>();

var backendGenerator  = new BackendGenerator(templateEngine, outputPath, verbose);
var frontendGenerator = new FrontendGenerator(templateEngine, outputPath, verbose);
var databaseGenerator = new DatabaseGenerator(templateEngine, outputPath, verbose);

generatedFiles.AddRange(await backendGenerator.GenerateAsync(project));
generatedFiles.AddRange(await frontendGenerator.GenerateAsync(project));
generatedFiles.AddRange(await databaseGenerator.GenerateAsync(project));

// ─── Step 5: Summary ─────────────────────────────────────────────────────
Console.WriteLine();
Console.Write("[ 4/4 ] Generation complete. ");
Console.ForegroundColor = ConsoleColor.Green;
Console.WriteLine($"{generatedFiles.Count} files written.");
Console.ResetColor();
Console.WriteLine();

Console.WriteLine("Generated files:");
foreach (var file in generatedFiles)
{
    var rel = Path.GetRelativePath(outputPath, file);
    Console.ForegroundColor = ConsoleColor.DarkGray;
    Console.Write("  → ");
    Console.ResetColor();
    Console.WriteLine(rel);
}

Console.WriteLine();
Console.ForegroundColor = ConsoleColor.Cyan;
Console.WriteLine($"Output directory: {Path.GetFullPath(outputPath)}");
Console.ResetColor();

return 0;

static void PrintHelp()
{
    Console.WriteLine();
    Console.WriteLine("Usage:");
    Console.WriteLine("  aicase --input <project.json> --output <output-dir> [options]");
    Console.WriteLine();
    Console.WriteLine("Options:");
    Console.WriteLine("  -i, --input      <path>   Path to project definition JSON   (required)");
    Console.WriteLine("  -o, --output     <path>   Output directory for generated code (required)");
    Console.WriteLine("  -t, --templates  <path>   Path to Scriban templates directory");
    Console.WriteLine("  -v, --verbose             Print detailed generation log");
    Console.WriteLine("  -h, --help                Show this help message");
    Console.WriteLine();
    Console.WriteLine("Examples:");
    Console.WriteLine("  aicase --input schema/ejemplo-proyecto.json --output ./out/SistemaVentas");
    Console.WriteLine("  aicase -i myproject.json -o ./generated -t ./custom-templates -v");
}
