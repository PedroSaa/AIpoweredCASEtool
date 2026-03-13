using Newtonsoft.Json.Linq;
using AicaseCli.Models;

namespace AicaseCli.Core;

/// <summary>
/// Parsea el JSON del proyecto y lo convierte en modelos internos del generador.
/// </summary>
public class ProjectParser
{
    /// <summary>
    /// Lee y parsea el archivo JSON del proyecto.
    /// </summary>
    public async Task<ProjectDefinition> ParseAsync(string jsonFilePath)
    {
        var json = await File.ReadAllTextAsync(jsonFilePath);
        var root = JObject.Parse(json);
        return ParseProject(root);
    }

    private static ProjectDefinition ParseProject(JObject root)
    {
        var project = new ProjectDefinition
        {
            Name = root["proyecto"]?.ToString() ?? "Proyecto",
            Version = root["version"]?.ToString() ?? "1.0",
            Description = root["descripcion"]?.ToString() ?? string.Empty
        };

        // Tecnologías
        if (root["tecnologias"] is JObject tech)
        {
            project.Technologies = new TechnologyStack
            {
                Backend = tech["backend"]?.ToString() ?? "C# .NET 8 Web API",
                Frontend = tech["frontend"]?.ToString() ?? "Angular 19",
                Database = tech["database"]?.ToString() ?? "SQL Server"
            };
        }

        // Configuración global
        if (root["configuracionGlobal"] is JObject global)
        {
            project.GlobalConfig = ParseGlobalConfig(global);
        }

        // Entidades
        if (root["entidades"] is JArray entities)
        {
            project.Entities = entities
                .OfType<JObject>()
                .Select(ParseEntity)
                .ToList();
        }

        return project;
    }

    private static GlobalConfig ParseGlobalConfig(JObject global)
    {
        var config = new GlobalConfig();

        if (global["autenticacion"] is JObject auth)
        {
            config.Authentication = new AuthConfig
            {
                Type = auth["tipo"]?.ToString() ?? "JWT",
                ExpirationMinutes = auth["expiracionMinutos"]?.Value<int>() ?? 60,
                RefreshToken = auth["refreshToken"]?.Value<bool>() ?? false
            };
        }

        if (global["roles"] is JArray roles)
        {
            config.Roles = roles.Select(r => r.ToString()).ToList();
        }

        if (global["baseDatos"] is JObject db)
        {
            config.DatabaseConfig = new DatabaseConfig
            {
                DatabaseName = db["nombreBaseDatos"]?.ToString() ?? string.Empty,
                Schema = db["esquema"]?.ToString() ?? "dbo",
                UseStoredProcedures = db["usarStoredProcedures"]?.Value<bool>() ?? false
            };
        }

        return config;
    }

    private static EntityDefinition ParseEntity(JObject entity)
    {
        var def = new EntityDefinition
        {
            Name = entity["nombre"]?.ToString() ?? "Entidad",
            TableName = entity["tabla"]?.ToString() ?? "Tabla",
            Description = entity["descripcion"]?.ToString() ?? string.Empty
        };

        // Campos
        if (entity["campos"] is JArray fields)
        {
            def.Fields = fields.OfType<JObject>().Select(ParseField).ToList();
        }

        // Relaciones
        if (entity["relaciones"] is JArray relations)
        {
            def.Relations = relations.OfType<JObject>().Select(ParseRelation).ToList();
        }

        // Reglas de negocio
        if (entity["reglasNegocio"] is JArray rules)
        {
            def.BusinessRules = rules.OfType<JObject>().Select(ParseBusinessRule).ToList();
        }

        // Pantallas
        if (entity["pantallas"] is JObject screens)
        {
            def.Screens = ParseScreens(screens);
        }

        // API
        if (entity["api"] is JObject api)
        {
            def.Api = ParseApi(api);
        }

        return def;
    }

    private static FieldDefinition ParseField(JObject field)
    {
        var def = new FieldDefinition
        {
            Name = field["nombre"]?.ToString() ?? "Campo",
            Type = field["tipo"]?.ToString() ?? "string",
            MaxLength = field["largo"]?.Value<int?>(),
            Precision = field["precision"]?.Value<int?>(),
            Scale = field["escala"]?.Value<int?>(),
            IsRequired = field["requerido"]?.Value<bool>() ?? false,
            IsUnique = field["unico"]?.Value<bool>() ?? false,
            IsPK = field["esPK"]?.Value<bool>() ?? false,
            AutoIncrement = field["autoIncremento"]?.Value<bool>() ?? false,
            IsFK = field["esFK"]?.Value<bool>() ?? false,
            DefaultValue = field["valorDefecto"]?.ToString()
        };

        // Validaciones
        if (field["validaciones"] is JArray validations)
        {
            def.Validations = validations.OfType<JObject>().Select(ParseValidation).ToList();
        }

        // Input UI
        if (field["input"] is JObject input)
        {
            def.Input = ParseInput(input);
        }

        return def;
    }

    private static ValidationRule ParseValidation(JObject v) => new()
    {
        Type = v["tipo"]?.ToString() ?? string.Empty,
        Value = v["valor"],
        Message = v["mensaje"]?.ToString(),
        RegexExpression = v["expresionRegex"]?.ToString()
    };

    private static UiInput ParseInput(JObject input)
    {
        var ui = new UiInput
        {
            Type = input["tipo"]?.ToString() ?? "text",
            Label = input["label"]?.ToString() ?? string.Empty,
            Placeholder = input["placeholder"]?.ToString(),
            DataSource = input["fuenteDatos"]?.ToString(),
            Order = input["orden"]?.Value<int?>(),
            HideInList = input["ocultarEnListado"]?.Value<bool>() ?? false
        };

        if (input["opciones"] is JArray options)
        {
            ui.Options = options.OfType<JObject>().Select(o => new InputOption
            {
                Value = o["valor"],
                Label = o["etiqueta"]?.ToString() ?? string.Empty
            }).ToList();
        }

        return ui;
    }

    private static RelationDefinition ParseRelation(JObject r) => new()
    {
        Name = r["nombre"]?.ToString() ?? string.Empty,
        Type = r["tipo"]?.ToString() ?? string.Empty,
        TargetEntity = r["entidadDestino"]?.ToString() ?? string.Empty,
        LocalField = r["campoLocal"]?.ToString() ?? string.Empty,
        TargetField = r["campoDestino"]?.ToString() ?? string.Empty,
        EagerLoading = r["cargaEager"]?.Value<bool>() ?? false,
        PivotTable = r["tablaPivote"]?.ToString()
    };

    private static BusinessRule ParseBusinessRule(JObject r) => new()
    {
        Name = r["nombre"]?.ToString() ?? string.Empty,
        Description = r["descripcion"]?.ToString() ?? string.Empty,
        Trigger = r["trigger"]?.ToString() ?? string.Empty,
        Condition = r["condicion"]?.ToString(),
        Action = r["accion"]?.ToString() ?? string.Empty,
        Message = r["mensaje"]?.ToString(),
        CustomCode = r["codigoPersonalizado"]?.ToString()
    };

    private static ScreenConfig ParseScreens(JObject screens)
    {
        var config = new ScreenConfig();

        if (screens["listado"] is JObject list)
        {
            config.List = new ListScreenConfig
            {
                Title = list["titulo"]?.ToString() ?? string.Empty,
                Columns = list["columnas"]?.ToObject<List<string>>() ?? [],
                Actions = list["acciones"]?.ToObject<List<string>>() ?? [],
                Filters = list["filtros"]?.ToObject<List<string>>() ?? []
            };

            if (list["paginacion"] is JObject pag)
            {
                config.List.Pagination = new PaginationConfig
                {
                    PageSize = pag["registrosPorPagina"]?.Value<int>() ?? 10,
                    PageSizeOptions = pag["opcionesPagina"]?.ToObject<List<int>>() ?? [5, 10, 25, 50]
                };
            }

            if (list["ordenamientoPorDefecto"] is JObject sort)
            {
                config.List.DefaultSort = new SortConfig
                {
                    Field = sort["campo"]?.ToString() ?? string.Empty,
                    Direction = sort["direccion"]?.ToString() ?? "asc"
                };
            }
        }

        if (screens["formulario"] is JObject form)
        {
            config.Form = new FormScreenConfig
            {
                Title = form["titulo"]?.ToString() ?? string.Empty,
                Layout = form["layout"]?.ToString() ?? "1-columna",
                Fields = form["campos"]?.ToObject<List<string>>() ?? []
            };

            if (form["secciones"] is JArray sections)
            {
                config.Form.Sections = sections.OfType<JObject>().Select(s => new FormSection
                {
                    Title = s["titulo"]?.ToString() ?? string.Empty,
                    Fields = s["campos"]?.ToObject<List<string>>() ?? [],
                    Columns = s["columnas"]?.Value<int>() ?? 1
                }).ToList();
            }
        }

        return config;
    }

    private static ApiConfig ParseApi(JObject api) => new()
    {
        Route = api["ruta"]?.ToString() ?? string.Empty,
        Operations = api["operaciones"]?.ToObject<List<string>>() ?? [],
        RequiresAuth = api["autenticacion"]?.Value<bool>() ?? true,
        Roles = api["roles"]?.ToObject<List<string>>() ?? [],
        Version = api["versionamiento"]?.ToString()
    };
}
