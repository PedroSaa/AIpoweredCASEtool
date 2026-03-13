using AicaseCli.Core;
using AicaseCli.Models;

namespace AicaseCli.Generators;

/// <summary>
/// Genera scripts SQL Server: CREATE TABLE y stored procedures CRUD para cada entidad.
/// </summary>
public class DatabaseGenerator
{
    private readonly TemplateEngine _engine;
    private readonly string _templatesPath;

    public DatabaseGenerator(TemplateEngine engine, string templatesPath)
    {
        _engine = engine;
        _templatesPath = templatesPath;
    }

    public async Task GenerateAsync(ProjectDefinition project, string outputBasePath)
    {
        var dbPath = Path.Combine(outputBasePath, "database");

        // Script principal que crea todas las tablas
        await GenerateCreateTablesScriptAsync(project, dbPath);

        // Stored procedures por entidad
        foreach (var entity in project.Entidades)
        {
            var model = BuildModel(project, entity);
            await GenerateStoredProceduresAsync(model, dbPath, entity.Nombre);
        }

        // Script de datos iniciales
        await GenerateSeedDataScriptAsync(project, dbPath);

        Console.WriteLine($"  → {project.Entidades.Count} tablas y sus SPs generados.");
    }

    private static object BuildModel(ProjectDefinition project, EntityDefinition entity) => new
    {
        Project = new
        {
            project.Nombre,
            project.Version,
            Schema = project.Configuracion?.BaseDatos?.Esquema ?? "dbo",
            Database = project.Configuracion?.BaseDatos?.NombreBaseDatos ?? project.Nombre + "DB"
        },
        Entity = entity
    };

    private async Task GenerateCreateTablesScriptAsync(ProjectDefinition project, string dbPath)
    {
        var outputPath = Path.Combine(dbPath, "01_CreateTables.sql");
        var templatePath = Path.Combine(_templatesPath, "database", "create-table.scriban");

        if (File.Exists(templatePath))
        {
            // Una entidad por llamada, concatenar resultados
            var allScripts = new List<string>
            {
                $"-- Script generado por AICASE para {project.Nombre}",
                $"-- Generado: {DateTime.UtcNow:yyyy-MM-dd HH:mm} UTC",
                "",
                $"USE [{project.Configuracion?.BaseDatos?.NombreBaseDatos ?? project.Nombre + "DB"}]",
                "GO",
                ""
            };

            foreach (var entity in project.Entidades)
            {
                var model = BuildModel(project, entity);
                var script = await _engine.RenderAsync(templatePath, model);
                allScripts.Add(script);
            }

            Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);
            await File.WriteAllTextAsync(outputPath, string.Join("\n", allScripts));
        }
        else
        {
            var allScripts = new List<string>
            {
                $"-- Script generado por AICASE para {project.Nombre}",
                $"-- Generado: {DateTime.UtcNow:yyyy-MM-dd HH:mm} UTC",
                $"-- Base de datos: {project.Configuracion?.BaseDatos?.NombreBaseDatos ?? project.Nombre + "DB"}",
                "",
                $"USE [{project.Configuracion?.BaseDatos?.NombreBaseDatos ?? project.Nombre + "DB"}]",
                "GO",
                ""
            };

            foreach (var entity in project.Entidades)
            {
                var model = BuildModel(project, entity);
                var script = await _engine.RenderFromStringAsync(GetEmbeddedCreateTableTemplate(), model);
                allScripts.Add(script);
                allScripts.Add("");
            }

            Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);
            await File.WriteAllTextAsync(outputPath, string.Join("\n", allScripts));
        }
    }

    private async Task GenerateStoredProceduresAsync(object model, string dbPath, string entityName)
    {
        var outputPath = Path.Combine(dbPath, "procedures", $"{entityName}_CRUD.sql");
        var templatePath = Path.Combine(_templatesPath, "database", "stored-procedure-crud.scriban");

        if (File.Exists(templatePath))
            await _engine.RenderToFileAsync(templatePath, model, outputPath);
        else
            await _engine.RenderStringToFileAsync(GetEmbeddedStoredProcedureTemplate(), model, outputPath);
    }

    private static async Task GenerateSeedDataScriptAsync(ProjectDefinition project, string dbPath)
    {
        var outputPath = Path.Combine(dbPath, "02_SeedData.sql");
        Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);

        var content = $"""
-- Script de datos iniciales para {project.Nombre}
-- Generado: {DateTime.UtcNow:yyyy-MM-dd HH:mm} UTC
-- Modifique este script para insertar datos de ejemplo o configuración inicial.

USE [{project.Configuracion?.BaseDatos?.NombreBaseDatos ?? project.Nombre + "DB"}]
GO

-- Ejemplo de datos iniciales:
-- INSERT INTO dbo.TiposCliente (Nombre, Descripcion, Activo) VALUES ('Minorista', 'Cliente minorista', 1);
-- INSERT INTO dbo.TiposCliente (Nombre, Descripcion, Activo) VALUES ('Mayorista', 'Cliente mayorista', 1);
-- INSERT INTO dbo.TiposCliente (Nombre, Descripcion, Activo) VALUES ('VIP', 'Cliente VIP con beneficios especiales', 1);
-- GO

PRINT 'Datos iniciales insertados correctamente.';
GO
""";
        await File.WriteAllTextAsync(outputPath, content);
    }

    // ── Plantillas embebidas ─────────────────────────────────────────────────

    private static string GetEmbeddedCreateTableTemplate() => """
-- ============================================================
-- Tabla: {{ entity.tabla }}
-- Entidad: {{ entity.nombre }}
-- ============================================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[{{ project.schema }}].[{{ entity.tabla }}]') AND type in (N'U'))
BEGIN
    CREATE TABLE [{{ project.schema }}].[{{ entity.tabla }}] (
        {{- campo_count = entity.campos.size
        for i in 0..(campo_count - 1)
            campo = entity.campos[i]
            is_last = i == campo_count - 1 }}
        [{{ campo.nombre }}] {{ campo.tipo | sql_type campo.largo campo.precision campo.escala }}{{ if campo.es_pk }} NOT NULL IDENTITY(1,1){{ else if campo.requerido }} NOT NULL{{ else }} NULL{{ end }}{{ if !is_last }},{{ end }}
        {{- end }}
    );

    -- Clave primaria
    {{- for campo in entity.campos }}
    {{- if campo.es_pk }}
    ALTER TABLE [{{ project.schema }}].[{{ entity.tabla }}]
        ADD CONSTRAINT [PK_{{ entity.tabla }}] PRIMARY KEY CLUSTERED ([{{ campo.nombre }}] ASC);
    {{- end }}
    {{- end }}

    -- Índices únicos
    {{- for campo in entity.campos }}
    {{- if campo.unico && !campo.es_pk }}
    CREATE UNIQUE NONCLUSTERED INDEX [UQ_{{ entity.tabla }}_{{ campo.nombre }}]
        ON [{{ project.schema }}].[{{ entity.tabla }}] ([{{ campo.nombre }}] ASC)
        WHERE ([{{ campo.nombre }}] IS NOT NULL);
    {{- end }}
    {{- end }}

    PRINT 'Tabla {{ entity.tabla }} creada correctamente.';
END
ELSE
    PRINT 'Tabla {{ entity.tabla }} ya existe. Omitiendo.';
GO

""";

    private static string GetEmbeddedStoredProcedureTemplate() => """
-- ============================================================
-- Stored Procedures CRUD para {{ entity.nombre }}
-- Tabla: [{{ project.schema }}].[{{ entity.tabla }}]
-- Generado por AICASE
-- ============================================================

USE [{{ project.database }}]
GO

-- ── sp_{{ entity.nombre }}_GetAll ───────────────────────────────────────
IF OBJECT_ID(N'[{{ project.schema }}].[sp_{{ entity.nombre }}_GetAll]', 'P') IS NOT NULL
    DROP PROCEDURE [{{ project.schema }}].[sp_{{ entity.nombre }}_GetAll];
GO

CREATE PROCEDURE [{{ project.schema }}].[sp_{{ entity.nombre }}_GetAll]
    @Page       INT = 1,
    @PageSize   INT = 10,
    @Search     NVARCHAR(200) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Offset INT = (@Page - 1) * @PageSize;

    SELECT COUNT(*) OVER() AS TotalCount,
           {{- for campo in entity.campos }}
           t.[{{ campo.nombre }}]{{ if !for.last }},{{ end }}
           {{- end }}
    FROM [{{ project.schema }}].[{{ entity.tabla }}] t
    WHERE (@Search IS NULL
        {{- for campo in entity.campos }}
        {{- if campo.tipo == "string" }}
        OR t.[{{ campo.nombre }}] LIKE N'%' + @Search + '%'
        {{- end }}
        {{- end }})
    ORDER BY t.{{ entity.pantallas.listado.ordenamiento ?? "Id" }}
    OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
END
GO

-- ── sp_{{ entity.nombre }}_GetById ──────────────────────────────────────
IF OBJECT_ID(N'[{{ project.schema }}].[sp_{{ entity.nombre }}_GetById]', 'P') IS NOT NULL
    DROP PROCEDURE [{{ project.schema }}].[sp_{{ entity.nombre }}_GetById];
GO

CREATE PROCEDURE [{{ project.schema }}].[sp_{{ entity.nombre }}_GetById]
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT {{- for campo in entity.campos }}
           [{{ campo.nombre }}]{{ if !for.last }},{{ end }}
           {{- end }}
    FROM [{{ project.schema }}].[{{ entity.tabla }}]
    WHERE [{{ entity.primary_key.nombre ?? "Id" }}] = @Id;
END
GO

-- ── sp_{{ entity.nombre }}_Insert ───────────────────────────────────────
IF OBJECT_ID(N'[{{ project.schema }}].[sp_{{ entity.nombre }}_Insert]', 'P') IS NOT NULL
    DROP PROCEDURE [{{ project.schema }}].[sp_{{ entity.nombre }}_Insert];
GO

CREATE PROCEDURE [{{ project.schema }}].[sp_{{ entity.nombre }}_Insert]
    {{- for campo in entity.campos }}
    {{- if !campo.es_pk }}
    @{{ campo.nombre }} {{ campo.tipo | sql_type campo.largo campo.precision campo.escala }}{{ if !campo.requerido }} = NULL{{ end }}{{ if !for.last }},{{ end }}
    {{- end }}
    {{- end }}
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO [{{ project.schema }}].[{{ entity.tabla }}] (
        {{- for campo in entity.campos }}
        {{- if !campo.es_pk }}
        [{{ campo.nombre }}]{{ if !for.last }},{{ end }}
        {{- end }}
        {{- end }}
    ) VALUES (
        {{- for campo in entity.campos }}
        {{- if !campo.es_pk }}
        @{{ campo.nombre }}{{ if !for.last }},{{ end }}
        {{- end }}
        {{- end }}
    );
    SELECT SCOPE_IDENTITY() AS NewId;
END
GO

-- ── sp_{{ entity.nombre }}_Update ───────────────────────────────────────
IF OBJECT_ID(N'[{{ project.schema }}].[sp_{{ entity.nombre }}_Update]', 'P') IS NOT NULL
    DROP PROCEDURE [{{ project.schema }}].[sp_{{ entity.nombre }}_Update];
GO

CREATE PROCEDURE [{{ project.schema }}].[sp_{{ entity.nombre }}_Update]
    @Id INT,
    {{- for campo in entity.campos }}
    {{- if !campo.es_pk }}
    @{{ campo.nombre }} {{ campo.tipo | sql_type campo.largo campo.precision campo.escala }}{{ if !campo.requerido }} = NULL{{ end }}{{ if !for.last }},{{ end }}
    {{- end }}
    {{- end }}
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE [{{ project.schema }}].[{{ entity.tabla }}]
    SET {{- for campo in entity.campos }}
        {{- if !campo.es_pk }}
        [{{ campo.nombre }}] = @{{ campo.nombre }}{{ if !for.last }},{{ end }}
        {{- end }}
        {{- end }}
    WHERE [{{ entity.primary_key.nombre ?? "Id" }}] = @Id;
    SELECT @@ROWCOUNT AS RowsAffected;
END
GO

-- ── sp_{{ entity.nombre }}_Delete ───────────────────────────────────────
IF OBJECT_ID(N'[{{ project.schema }}].[sp_{{ entity.nombre }}_Delete]', 'P') IS NOT NULL
    DROP PROCEDURE [{{ project.schema }}].[sp_{{ entity.nombre }}_Delete];
GO

CREATE PROCEDURE [{{ project.schema }}].[sp_{{ entity.nombre }}_Delete]
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    DELETE FROM [{{ project.schema }}].[{{ entity.tabla }}]
    WHERE [{{ entity.primary_key.nombre ?? "Id" }}] = @Id;
    SELECT @@ROWCOUNT AS RowsAffected;
END
GO

PRINT 'Stored procedures de {{ entity.nombre }} creados correctamente.';
GO
""";
}
