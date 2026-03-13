using Newtonsoft.Json;

namespace AicaseCli.Models;

public class ApiConfig
{
    [JsonProperty("ruta")]
    public string Ruta { get; set; } = string.Empty;

    [JsonProperty("operaciones")]
    public List<string> Operaciones { get; set; } = new();

    [JsonProperty("autenticacion")]
    public bool Autenticacion { get; set; } = true;

    [JsonProperty("roles")]
    public List<string> Roles { get; set; } = new();

    [JsonProperty("versionApi")]
    public string? VersionApi { get; set; }
}
