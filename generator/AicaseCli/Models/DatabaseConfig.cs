using Newtonsoft.Json;

namespace AicaseCli.Models;

public class DatabaseConfig
{
    [JsonProperty("servidor")]
    public string Servidor { get; set; } = "localhost";

    [JsonProperty("nombreBaseDatos")]
    public string NombreBaseDatos { get; set; } = string.Empty;

    [JsonProperty("usuario")]
    public string? Usuario { get; set; }

    [JsonProperty("contrasena")]
    public string? Contrasena { get; set; }

    [JsonProperty("puerto")]
    public int? Puerto { get; set; }

    [JsonProperty("esquema")]
    public string Esquema { get; set; } = "dbo";
}
