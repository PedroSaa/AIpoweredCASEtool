using Newtonsoft.Json;

namespace AicaseCli.Models;

public class GlobalConfig
{
    [JsonProperty("autenticacion")]
    public AuthConfig? Autenticacion { get; set; }

    [JsonProperty("roles")]
    public List<string> Roles { get; set; } = new();

    [JsonProperty("baseDatos")]
    public DatabaseConfig? BaseDatos { get; set; }

    [JsonProperty("namespaceBackend")]
    public string? NamespaceBackend { get; set; }

    [JsonProperty("nombreFrontend")]
    public string? NombreFrontend { get; set; }

    [JsonProperty("cors")]
    public List<string> Cors { get; set; } = new();
}
