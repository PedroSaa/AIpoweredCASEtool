namespace AicaseCli.Models;

/// <summary>
/// Definición de una entidad del dominio.
/// </summary>
public class EntityDefinition
{
    public string Name { get; set; } = string.Empty;
    public string TableName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<FieldDefinition> Fields { get; set; } = [];
    public List<RelationDefinition> Relations { get; set; } = [];
    public List<BusinessRule> BusinessRules { get; set; } = [];
    public ScreenConfig? Screens { get; set; }
    public ApiConfig? Api { get; set; }

    /// <summary>Campo que es PK de la entidad.</summary>
    public FieldDefinition? PrimaryKey => Fields.FirstOrDefault(f => f.IsPK);

    /// <summary>Campos que no son PK ni auto-increment (para Create DTO).</summary>
    public IEnumerable<FieldDefinition> CreateFields => Fields.Where(f => !f.IsPK || !f.AutoIncrement);

    /// <summary>Campos que no son PK (para Update DTO).</summary>
    public IEnumerable<FieldDefinition> UpdateFields => Fields.Where(f => !f.IsPK);

    /// <summary>Nombre del controlador (plural + "Controller").</summary>
    public string ControllerName => $"{Name}sController";

    /// <summary>Nombre del archivo Angular en kebab-case.</summary>
    public string AngularFileName => ToKebabCase(Name);

    private static string ToKebabCase(string name)
    {
        return string.Concat(name.Select((c, i) =>
            i > 0 && char.IsUpper(c) ? "-" + char.ToLower(c) : char.ToLower(c).ToString()));
    }
}

/// <summary>
/// Definición de un campo/columna.
/// </summary>
public class FieldDefinition
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = "string";
    public int? MaxLength { get; set; }
    public int? Precision { get; set; }
    public int? Scale { get; set; }
    public bool IsRequired { get; set; }
    public bool IsUnique { get; set; }
    public bool IsPK { get; set; }
    public bool AutoIncrement { get; set; }
    public bool IsFK { get; set; }
    public object? DefaultValue { get; set; }
    public List<ValidationRule> Validations { get; set; } = [];
    public UiInput? Input { get; set; }

    /// <summary>Tipo C# mapeado desde el tipo del JSON.</summary>
    public string CSharpType => TypeMapper.ToCSharp(Type, IsRequired, MaxLength);

    /// <summary>Tipo TypeScript mapeado.</summary>
    public string TypeScriptType => TypeMapper.ToTypeScript(Type);

    /// <summary>Tipo SQL Server mapeado.</summary>
    public string SqlType => TypeMapper.ToSql(Type, MaxLength, Precision, Scale);
}

/// <summary>
/// Regla de validación de un campo.
/// </summary>
public class ValidationRule
{
    public string Type { get; set; } = string.Empty;
    public object? Value { get; set; }
    public string? Message { get; set; }
    public string? RegexExpression { get; set; }
}

/// <summary>
/// Configuración del input de UI.
/// </summary>
public class UiInput
{
    public string Type { get; set; } = "text";
    public string Label { get; set; } = string.Empty;
    public string? Placeholder { get; set; }
    public string? DataSource { get; set; }
    public List<InputOption> Options { get; set; } = [];
    public int? Order { get; set; }
    public bool HideInList { get; set; }
}

public class InputOption
{
    public object? Value { get; set; }
    public string Label { get; set; } = string.Empty;
}

/// <summary>
/// Relación entre entidades.
/// </summary>
public class RelationDefinition
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string TargetEntity { get; set; } = string.Empty;
    public string LocalField { get; set; } = string.Empty;
    public string TargetField { get; set; } = string.Empty;
    public bool EagerLoading { get; set; }
    public string? PivotTable { get; set; }
}

/// <summary>
/// Regla de negocio de la entidad.
/// </summary>
public class BusinessRule
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Trigger { get; set; } = string.Empty;
    public string? Condition { get; set; }
    public string Action { get; set; } = string.Empty;
    public string? Message { get; set; }
    public string? CustomCode { get; set; }
}

/// <summary>
/// Configuración de pantallas (UI).
/// </summary>
public class ScreenConfig
{
    public ListScreenConfig? List { get; set; }
    public FormScreenConfig? Form { get; set; }
}

public class ListScreenConfig
{
    public string Title { get; set; } = string.Empty;
    public List<string> Columns { get; set; } = [];
    public List<string> Actions { get; set; } = [];
    public List<string> Filters { get; set; } = [];
    public PaginationConfig Pagination { get; set; } = new();
    public SortConfig? DefaultSort { get; set; }
}

public class PaginationConfig
{
    public int PageSize { get; set; } = 10;
    public List<int> PageSizeOptions { get; set; } = [5, 10, 25, 50];
}

public class SortConfig
{
    public string Field { get; set; } = string.Empty;
    public string Direction { get; set; } = "asc";
}

public class FormScreenConfig
{
    public string Title { get; set; } = string.Empty;
    public string Layout { get; set; } = "1-columna";
    public List<string> Fields { get; set; } = [];
    public List<FormSection> Sections { get; set; } = [];
}

public class FormSection
{
    public string Title { get; set; } = string.Empty;
    public List<string> Fields { get; set; } = [];
    public int Columns { get; set; } = 1;
}

/// <summary>
/// Configuración de API REST.
/// </summary>
public class ApiConfig
{
    public string Route { get; set; } = string.Empty;
    public List<string> Operations { get; set; } = [];
    public bool RequiresAuth { get; set; } = true;
    public List<string> Roles { get; set; } = [];
    public string? Version { get; set; }
}
