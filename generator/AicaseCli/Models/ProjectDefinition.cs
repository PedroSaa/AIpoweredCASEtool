namespace AicaseCli.Models;

// ─── Root ─────────────────────────────────────────────────────────────────
public sealed class ProjectDefinition
{
    public ProjectMetadata    Project      { get; set; } = new();
    public GlobalConfig       GlobalConfig { get; set; } = new();
    public List<EntityDefinition> Entities { get; set; } = [];
}

// ─── Project metadata ─────────────────────────────────────────────────────
public sealed class ProjectMetadata
{
    public string           Name         { get; set; } = string.Empty;
    public string           Version      { get; set; } = "1.0.0";
    public string           Description  { get; set; } = string.Empty;
    public TechnologyConfig Technologies { get; set; } = new();
}

public sealed class TechnologyConfig
{
    public string Backend  { get; set; } = "dotnet8";
    public string Frontend { get; set; } = "angular17";
    public string Database { get; set; } = "sqlserver";
}

// ─── Global Config ────────────────────────────────────────────────────────
public sealed class GlobalConfig
{
    public AuthConfig          Authentication { get; set; } = new();
    public List<RoleDefinition> Roles         { get; set; } = [];
    public DatabaseConfig      Database       { get; set; } = new();
    public List<string>        AllowedOrigins { get; set; } = [];
}

public sealed class AuthConfig
{
    public bool   Enabled             { get; set; } = true;
    public string Type                { get; set; } = "jwt";
    public string JwtSecret           { get; set; } = string.Empty;
    public int    JwtExpireMinutes    { get; set; } = 60;
    public bool   RefreshTokenEnabled { get; set; } = true;
}

public sealed class RoleDefinition
{
    public string Name        { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool   IsDefault   { get; set; } = false;
}

public sealed class DatabaseConfig
{
    public string Schema                      { get; set; } = "dbo";
    public string ConnectionStringPlaceholder { get; set; } = string.Empty;
    public bool   EnableMigrations            { get; set; } = true;
    public bool   EnableSeedData              { get; set; } = false;
}

// ─── Entity ───────────────────────────────────────────────────────────────
public sealed class EntityDefinition
{
    public string Name            { get; set; } = string.Empty;
    public string Table           { get; set; } = string.Empty;
    public string Description     { get; set; } = string.Empty;
    public List<FieldDefinition>  Fields        { get; set; } = [];
    public List<RelationDefinition> Relations   { get; set; } = [];
    public List<BusinessRule>     BusinessRules { get; set; } = [];
    public ScreenConfig?          ScreenConfig  { get; set; }
    public ApiConfig?             ApiConfig     { get; set; }

    // Computed helpers
    public FieldDefinition? PrimaryKey => Fields.FirstOrDefault(f => f.IsPK);
    public string NameLower => char.ToLowerInvariant(Name[0]) + Name[1..];
    public string NamePlural => Name.EndsWith("s") ? Name + "es" : Name + "s";
    public string KebabName => AicaseCli.Core.TemplateEngine.ToKebabCase(Name);
}

// ─── Field ────────────────────────────────────────────────────────────────
public sealed class FieldDefinition
{
    public string           Name          { get; set; } = string.Empty;
    public string           Type          { get; set; } = "string";
    public int?             Length        { get; set; }
    public int?             Precision     { get; set; }
    public int?             Scale         { get; set; }
    public bool             Required      { get; set; } = false;
    public bool             Unique        { get; set; } = false;
    public bool             IsPK          { get; set; } = false;
    public bool             AutoIncrement { get; set; } = false;
    public string?          DefaultValue  { get; set; }
    public string           Description   { get; set; } = string.Empty;
    public FieldValidation? Validation    { get; set; }
    public UiInput?         Ui            { get; set; }

    // Computed helpers
    public string CSharpType => AicaseCli.Core.TemplateEngine.ToCSharpType(Type, Required);
    public string TypeScriptType => AicaseCli.Core.TemplateEngine.ToTypeScriptType(Type);
    public string SqlType => AicaseCli.Core.TemplateEngine.ToSqlType(Type, Length, Precision, Scale);
    public string NamePascal => char.ToUpperInvariant(Name[0]) + Name[1..];
}

// ─── Validation ───────────────────────────────────────────────────────────
public sealed class FieldValidation
{
    public int?    MinLength    { get; set; }
    public int?    MaxLength    { get; set; }
    public double? Min          { get; set; }
    public double? Max          { get; set; }
    public bool?   Email        { get; set; }
    public string? Regex        { get; set; }
    public string? RegexMessage { get; set; }
    public string? MaxDate      { get; set; }
    public string? MinDate      { get; set; }
    public string? Custom       { get; set; }
}

// ─── UI Input ─────────────────────────────────────────────────────────────
public sealed class UiInput
{
    public string       Type        { get; set; } = "text";
    public string       Label       { get; set; } = string.Empty;
    public string       Placeholder { get; set; } = string.Empty;
    public string?      Hint        { get; set; }
    public bool         Readonly    { get; set; } = false;
    public bool         Hidden      { get; set; } = false;
    public UiDataSource? DataSource { get; set; }
}

public sealed class UiDataSource
{
    public string?           Entity        { get; set; }
    public string?           ValueField    { get; set; }
    public string?           LabelField    { get; set; }
    public List<StaticOption> StaticOptions { get; set; } = [];
}

public sealed class StaticOption
{
    public string Value { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
}

// ─── Relation ─────────────────────────────────────────────────────────────
public sealed class RelationDefinition
{
    public string  Type        { get; set; } = "ManyToOne";
    public string  Target      { get; set; } = string.Empty;
    public string? Field       { get; set; }
    public string  TargetField { get; set; } = "id";
    public string? JoinTable   { get; set; }
    public bool    Cascade     { get; set; } = false;
    public bool    Nullable    { get; set; } = true;
    public bool    Eager       { get; set; } = false;
}

// ─── Business Rule ────────────────────────────────────────────────────────
public sealed class BusinessRule
{
    public string  Name        { get; set; } = string.Empty;
    public string  Description { get; set; } = string.Empty;
    public string  Trigger     { get; set; } = "beforeInsert";
    public string  Condition   { get; set; } = "true";
    public string  Action      { get; set; } = "throw";
    public string? Message     { get; set; }
    public string? TargetField { get; set; }
    public string? TargetValue { get; set; }
    public bool    AiGenerate  { get; set; } = false;
}

// ─── Screen Config ────────────────────────────────────────────────────────
public sealed class ScreenConfig
{
    public ListConfig? List { get; set; }
    public FormConfig? Form { get; set; }
}

public sealed class ListConfig
{
    public string            Title      { get; set; } = string.Empty;
    public List<ColumnConfig>  Columns  { get; set; } = [];
    public List<ActionConfig>  Actions  { get; set; } = [];
    public List<FilterConfig>  Filters  { get; set; } = [];
    public PaginationConfig  Pagination { get; set; } = new();
    public SortConfig?       Sort       { get; set; }
}

public sealed class ColumnConfig
{
    public string  Field      { get; set; } = string.Empty;
    public string  Header     { get; set; } = string.Empty;
    public bool    Sortable   { get; set; } = true;
    public bool    Filterable { get; set; } = false;
    public string? Width      { get; set; }
    public string? Pipe       { get; set; }
}

public sealed class ActionConfig
{
    public string       Name  { get; set; } = string.Empty;
    public string       Label { get; set; } = string.Empty;
    public string?      Icon  { get; set; }
    public string       Color { get; set; } = "default";
    public List<string> Roles { get; set; } = [];
}

public sealed class FilterConfig
{
    public string Field { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string Type  { get; set; } = "text";
}

public sealed class PaginationConfig
{
    public bool        Enabled         { get; set; } = true;
    public int         PageSize        { get; set; } = 10;
    public List<int>   PageSizeOptions { get; set; } = [5, 10, 25];
}

public sealed class SortConfig
{
    public string Field     { get; set; } = string.Empty;
    public string Direction { get; set; } = "asc";
}

public sealed class FormConfig
{
    public string             Title    { get; set; } = string.Empty;
    public string             Layout   { get; set; } = "two-columns";
    public List<FormSection>  Sections { get; set; } = [];
    public List<FormFieldConfig> Fields { get; set; } = [];
}

public sealed class FormSection
{
    public string       Title       { get; set; } = string.Empty;
    public string?      Icon        { get; set; }
    public List<string> Fields      { get; set; } = [];
    public bool         Collapsible { get; set; } = false;
}

public sealed class FormFieldConfig
{
    public string Name    { get; set; } = string.Empty;
    public int    Colspan { get; set; } = 1;
    public int?   Order   { get; set; }
}

// ─── API Config ───────────────────────────────────────────────────────────
public sealed class ApiConfig
{
    public string          Route          { get; set; } = string.Empty;
    public List<string>    Operations     { get; set; } = [];
    public bool            Authentication { get; set; } = true;
    public string          Versioning     { get; set; } = "v1";
    public ApiRoleConfig   Roles          { get; set; } = new();
    public RateLimitConfig RateLimiting   { get; set; } = new();
}

public sealed class ApiRoleConfig
{
    public List<string> GetAll  { get; set; } = [];
    public List<string> GetById { get; set; } = [];
    public List<string> Create  { get; set; } = [];
    public List<string> Update  { get; set; } = [];
    public List<string> Delete  { get; set; } = [];
}

public sealed class RateLimitConfig
{
    public bool Enabled           { get; set; } = false;
    public int  RequestsPerMinute { get; set; } = 60;
}
