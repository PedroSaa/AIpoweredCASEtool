namespace AicaseCli.Models;

/// <summary>
/// Representación interna del proyecto parseado desde el JSON maestro.
/// </summary>
public class ProjectDefinition
{
    public string Name { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public TechnologyStack Technologies { get; set; } = new();
    public GlobalConfig GlobalConfig { get; set; } = new();
    public List<EntityDefinition> Entities { get; set; } = [];

    /// <summary>Namespace C# derivado del nombre del proyecto (sin espacios ni caracteres especiales).</summary>
    public string CSharpNamespace => Name.Replace(" ", "").Replace("-", "");

    /// <summary>Nombre en camelCase para Angular.</summary>
    public string AngularName => char.ToLower(CSharpNamespace[0]) + CSharpNamespace[1..];
}

public class TechnologyStack
{
    public string Backend { get; set; } = "C# .NET 8 Web API";
    public string Frontend { get; set; } = "Angular 19";
    public string Database { get; set; } = "SQL Server";
}

public class GlobalConfig
{
    public AuthConfig Authentication { get; set; } = new();
    public List<string> Roles { get; set; } = [];
    public DatabaseConfig DatabaseConfig { get; set; } = new();
}

public class AuthConfig
{
    public string Type { get; set; } = "JWT";
    public int ExpirationMinutes { get; set; } = 60;
    public bool RefreshToken { get; set; } = false;
}

public class DatabaseConfig
{
    public string DatabaseName { get; set; } = string.Empty;
    public string Schema { get; set; } = "dbo";
    public bool UseStoredProcedures { get; set; } = false;
}
