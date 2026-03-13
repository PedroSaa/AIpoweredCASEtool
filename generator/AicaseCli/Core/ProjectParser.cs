using System.Text.Json;
using System.Text.Json.Nodes;
using AicaseCli.Models;

namespace AicaseCli.Core;

/// <summary>
/// Parses the AICASE project JSON into strongly-typed internal models.
/// Uses System.Text.Json with a lenient options set.
/// </summary>
public static class ProjectParser
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling         = JsonCommentHandling.Skip,
        AllowTrailingCommas         = true
    };

    public static ProjectDefinition Parse(string json)
    {
        var root = JsonNode.Parse(json)
            ?? throw new InvalidOperationException("JSON root is null.");

        var definition = new ProjectDefinition
        {
            Project      = ParseProject(root["project"]!),
            GlobalConfig = ParseGlobalConfig(root["globalConfig"]),
            Entities     = ParseEntities(root["entities"])
        };

        return definition;
    }

    // ─────────────────────────────────────────────────────────────────────
    // Project
    // ─────────────────────────────────────────────────────────────────────
    private static ProjectMetadata ParseProject(JsonNode node)
    {
        var tech = node["technologies"];
        return new ProjectMetadata
        {
            Name        = node["name"]!.GetValue<string>(),
            Version     = node["version"]!.GetValue<string>(),
            Description = node["description"]?.GetValue<string>() ?? string.Empty,
            Technologies = new TechnologyConfig
            {
                Backend  = tech?["backend"]?.GetValue<string>()  ?? "dotnet8",
                Frontend = tech?["frontend"]?.GetValue<string>() ?? "angular17",
                Database = tech?["database"]?.GetValue<string>() ?? "sqlserver"
            }
        };
    }

    // ─────────────────────────────────────────────────────────────────────
    // GlobalConfig
    // ─────────────────────────────────────────────────────────────────────
    private static GlobalConfig ParseGlobalConfig(JsonNode? node)
    {
        if (node is null) return new GlobalConfig();

        var auth = node["authentication"];
        var db   = node["database"];
        var cors = node["cors"];

        return new GlobalConfig
        {
            Authentication = new AuthConfig
            {
                Enabled              = auth?["enabled"]?.GetValue<bool>()           ?? true,
                Type                 = auth?["type"]?.GetValue<string>()            ?? "jwt",
                JwtSecret            = auth?["jwtSecret"]?.GetValue<string>()       ?? string.Empty,
                JwtExpireMinutes     = auth?["jwtExpireMinutes"]?.GetValue<int>()   ?? 60,
                RefreshTokenEnabled  = auth?["refreshTokenEnabled"]?.GetValue<bool>() ?? true
            },
            Roles = ParseRoles(node["roles"]),
            Database = new DatabaseConfig
            {
                Schema                       = db?["schema"]?.GetValue<string>()                            ?? "dbo",
                ConnectionStringPlaceholder  = db?["connectionStringPlaceholder"]?.GetValue<string>()       ?? string.Empty,
                EnableMigrations             = db?["enableMigrations"]?.GetValue<bool>()                    ?? true,
                EnableSeedData               = db?["enableSeedData"]?.GetValue<bool>()                      ?? false
            },
            AllowedOrigins = cors?["allowedOrigins"]?.AsArray()
                .Select(o => o!.GetValue<string>()).ToList() ?? []
        };
    }

    private static List<RoleDefinition> ParseRoles(JsonNode? node)
    {
        if (node is not JsonArray arr) return [];
        return arr.Select(r => new RoleDefinition
        {
            Name        = r!["name"]!.GetValue<string>(),
            Description = r["description"]?.GetValue<string>() ?? string.Empty,
            IsDefault   = r["isDefault"]?.GetValue<bool>() ?? false
        }).ToList();
    }

    // ─────────────────────────────────────────────────────────────────────
    // Entities
    // ─────────────────────────────────────────────────────────────────────
    private static List<EntityDefinition> ParseEntities(JsonNode? node)
    {
        if (node is not JsonArray arr) return [];
        return arr.Select(e => ParseEntity(e!)).ToList();
    }

    private static EntityDefinition ParseEntity(JsonNode node)
    {
        return new EntityDefinition
        {
            Name          = node["name"]!.GetValue<string>(),
            Table         = node["table"]!.GetValue<string>(),
            Description   = node["description"]?.GetValue<string>() ?? string.Empty,
            Fields        = ParseFields(node["fields"]),
            Relations     = ParseRelations(node["relations"]),
            BusinessRules = ParseBusinessRules(node["businessRules"]),
            ScreenConfig  = ParseScreenConfig(node["screenConfig"]),
            ApiConfig     = ParseApiConfig(node["apiConfig"])
        };
    }

    // ─────────────────────────────────────────────────────────────────────
    // Fields
    // ─────────────────────────────────────────────────────────────────────
    private static List<FieldDefinition> ParseFields(JsonNode? node)
    {
        if (node is not JsonArray arr) return [];
        return arr.Select(f => ParseField(f!)).ToList();
    }

    private static FieldDefinition ParseField(JsonNode node)
    {
        return new FieldDefinition
        {
            Name          = node["name"]!.GetValue<string>(),
            Type          = node["type"]!.GetValue<string>(),
            Length        = node["length"]?.GetValue<int?>(),
            Precision     = node["precision"]?.GetValue<int?>(),
            Scale         = node["scale"]?.GetValue<int?>(),
            Required      = node["required"]?.GetValue<bool>()      ?? false,
            Unique        = node["unique"]?.GetValue<bool>()         ?? false,
            IsPK          = node["isPK"]?.GetValue<bool>()           ?? false,
            AutoIncrement = node["autoIncrement"]?.GetValue<bool>()  ?? false,
            DefaultValue  = node["defaultValue"]?.ToString(),
            Description   = node["description"]?.GetValue<string>() ?? string.Empty,
            Validation    = ParseValidation(node["validation"]),
            Ui            = ParseUiInput(node["ui"])
        };
    }

    private static FieldValidation? ParseValidation(JsonNode? node)
    {
        if (node is null) return null;
        return new FieldValidation
        {
            MinLength    = node["minLength"]?.GetValue<int?>(),
            MaxLength    = node["maxLength"]?.GetValue<int?>(),
            Min          = node["min"]?.GetValue<double?>(),
            Max          = node["max"]?.GetValue<double?>(),
            Email        = node["email"]?.GetValue<bool?>(),
            Regex        = node["regex"]?.GetValue<string>(),
            RegexMessage = node["regexMessage"]?.GetValue<string>(),
            MaxDate      = node["maxDate"]?.GetValue<string>(),
            MinDate      = node["minDate"]?.GetValue<string>(),
            Custom       = node["custom"]?.GetValue<string>()
        };
    }

    private static UiInput? ParseUiInput(JsonNode? node)
    {
        if (node is null) return null;
        var ds = node["dataSource"];
        UiDataSource? dataSource = null;
        if (ds is not null)
        {
            dataSource = new UiDataSource
            {
                Entity      = ds["entity"]?.GetValue<string>(),
                ValueField  = ds["valueField"]?.GetValue<string>(),
                LabelField  = ds["labelField"]?.GetValue<string>(),
                StaticOptions = ds["staticOptions"]?.AsArray()
                    .Select(o => new StaticOption
                    {
                        Value = o!["value"]?.ToString() ?? string.Empty,
                        Label = o["label"]!.GetValue<string>()
                    }).ToList() ?? []
            };
        }

        return new UiInput
        {
            Type        = node["type"]?.GetValue<string>()        ?? "text",
            Label       = node["label"]?.GetValue<string>()       ?? string.Empty,
            Placeholder = node["placeholder"]?.GetValue<string>() ?? string.Empty,
            Hint        = node["hint"]?.GetValue<string>(),
            Readonly    = node["readonly"]?.GetValue<bool>()      ?? false,
            Hidden      = node["hidden"]?.GetValue<bool>()        ?? false,
            DataSource  = dataSource
        };
    }

    // ─────────────────────────────────────────────────────────────────────
    // Relations
    // ─────────────────────────────────────────────────────────────────────
    private static List<RelationDefinition> ParseRelations(JsonNode? node)
    {
        if (node is not JsonArray arr) return [];
        return arr.Select(r => new RelationDefinition
        {
            Type        = r!["type"]!.GetValue<string>(),
            Target      = r["target"]!.GetValue<string>(),
            Field       = r["field"]?.GetValue<string>(),
            TargetField = r["targetField"]?.GetValue<string>() ?? "id",
            JoinTable   = r["joinTable"]?.GetValue<string>(),
            Cascade     = r["cascade"]?.GetValue<bool>()  ?? false,
            Nullable    = r["nullable"]?.GetValue<bool>() ?? true,
            Eager       = r["eager"]?.GetValue<bool>()    ?? false
        }).ToList();
    }

    // ─────────────────────────────────────────────────────────────────────
    // Business Rules
    // ─────────────────────────────────────────────────────────────────────
    private static List<BusinessRule> ParseBusinessRules(JsonNode? node)
    {
        if (node is not JsonArray arr) return [];
        return arr.Select(r => new BusinessRule
        {
            Name         = r!["name"]!.GetValue<string>(),
            Description  = r["description"]?.GetValue<string>() ?? string.Empty,
            Trigger      = r["trigger"]!.GetValue<string>(),
            Condition    = r["condition"]?.GetValue<string>()   ?? "true",
            Action       = r["action"]?.GetValue<string>()      ?? "throw",
            Message      = r["message"]?.GetValue<string>(),
            TargetField  = r["targetField"]?.GetValue<string>(),
            TargetValue  = r["targetValue"]?.ToString(),
            AiGenerate   = r["aiGenerate"]?.GetValue<bool>()    ?? false
        }).ToList();
    }

    // ─────────────────────────────────────────────────────────────────────
    // ScreenConfig
    // ─────────────────────────────────────────────────────────────────────
    private static ScreenConfig? ParseScreenConfig(JsonNode? node)
    {
        if (node is null) return null;
        return new ScreenConfig
        {
            List = ParseListConfig(node["list"]),
            Form = ParseFormConfig(node["form"])
        };
    }

    private static ListConfig? ParseListConfig(JsonNode? node)
    {
        if (node is null) return null;
        var pag = node["pagination"];
        var sort = node["sort"];
        return new ListConfig
        {
            Title   = node["title"]?.GetValue<string>() ?? string.Empty,
            Columns = node["columns"]?.AsArray().Select(c => new ColumnConfig
            {
                Field      = c!["field"]!.GetValue<string>(),
                Header     = c["header"]?.GetValue<string>() ?? c["field"]!.GetValue<string>(),
                Sortable   = c["sortable"]?.GetValue<bool>()   ?? true,
                Filterable = c["filterable"]?.GetValue<bool>() ?? false,
                Width      = c["width"]?.GetValue<string>(),
                Pipe       = c["pipe"]?.GetValue<string>()
            }).ToList() ?? [],
            Actions = node["actions"]?.AsArray().Select(a => new ActionConfig
            {
                Name  = a!["name"]!.GetValue<string>(),
                Label = a["label"]!.GetValue<string>(),
                Icon  = a["icon"]?.GetValue<string>(),
                Color = a["color"]?.GetValue<string>() ?? "default",
                Roles = a["roles"]?.AsArray().Select(r => r!.GetValue<string>()).ToList() ?? []
            }).ToList() ?? [],
            Filters = node["filters"]?.AsArray().Select(f => new FilterConfig
            {
                Field = f!["field"]!.GetValue<string>(),
                Label = f["label"]?.GetValue<string>() ?? f["field"]!.GetValue<string>(),
                Type  = f["type"]?.GetValue<string>() ?? "text"
            }).ToList() ?? [],
            Pagination = pag is null ? new PaginationConfig() : new PaginationConfig
            {
                Enabled         = pag["enabled"]?.GetValue<bool>()  ?? true,
                PageSize        = pag["pageSize"]?.GetValue<int>()  ?? 10,
                PageSizeOptions = pag["pageSizeOptions"]?.AsArray()
                    .Select(o => o!.GetValue<int>()).ToList() ?? [5, 10, 25]
            },
            Sort = sort is null ? null : new SortConfig
            {
                Field     = sort["field"]?.GetValue<string>() ?? string.Empty,
                Direction = sort["direction"]?.GetValue<string>() ?? "asc"
            }
        };
    }

    private static FormConfig? ParseFormConfig(JsonNode? node)
    {
        if (node is null) return null;
        return new FormConfig
        {
            Title    = node["title"]?.GetValue<string>() ?? string.Empty,
            Layout   = node["layout"]?.GetValue<string>() ?? "two-columns",
            Sections = node["sections"]?.AsArray().Select(s => new FormSection
            {
                Title       = s!["title"]!.GetValue<string>(),
                Icon        = s["icon"]?.GetValue<string>(),
                Fields      = s["fields"]?.AsArray().Select(f => f!.GetValue<string>()).ToList() ?? [],
                Collapsible = s["collapsible"]?.GetValue<bool>() ?? false
            }).ToList() ?? [],
            Fields = node["fields"]?.AsArray().Select(f => new FormFieldConfig
            {
                Name    = f!["name"]!.GetValue<string>(),
                Colspan = f["colspan"]?.GetValue<int>() ?? 1,
                Order   = f["order"]?.GetValue<int?>()
            }).ToList() ?? []
        };
    }

    // ─────────────────────────────────────────────────────────────────────
    // ApiConfig
    // ─────────────────────────────────────────────────────────────────────
    private static ApiConfig? ParseApiConfig(JsonNode? node)
    {
        if (node is null) return null;
        var roles = node["roles"];
        var rl    = node["rateLimiting"];
        return new ApiConfig
        {
            Route          = node["route"]?.GetValue<string>() ?? string.Empty,
            Operations     = node["operations"]?.AsArray().Select(o => o!.GetValue<string>()).ToList() ?? [],
            Authentication = node["authentication"]?.GetValue<bool>() ?? true,
            Versioning     = node["versioning"]?.GetValue<string>() ?? "v1",
            Roles = new ApiRoleConfig
            {
                GetAll  = roles?["getAll"]?.AsArray().Select(r => r!.GetValue<string>()).ToList() ?? [],
                GetById = roles?["getById"]?.AsArray().Select(r => r!.GetValue<string>()).ToList() ?? [],
                Create  = roles?["create"]?.AsArray().Select(r => r!.GetValue<string>()).ToList() ?? [],
                Update  = roles?["update"]?.AsArray().Select(r => r!.GetValue<string>()).ToList() ?? [],
                Delete  = roles?["delete"]?.AsArray().Select(r => r!.GetValue<string>()).ToList() ?? []
            },
            RateLimiting = new RateLimitConfig
            {
                Enabled           = rl?["enabled"]?.GetValue<bool>()           ?? false,
                RequestsPerMinute = rl?["requestsPerMinute"]?.GetValue<int>() ?? 60
            }
        };
    }
}
