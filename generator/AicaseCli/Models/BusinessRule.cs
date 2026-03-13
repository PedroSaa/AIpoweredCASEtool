using Newtonsoft.Json;

namespace AicaseCli.Models;

public class BusinessRule
{
    [JsonProperty("nombre")]
    public string Nombre { get; set; } = string.Empty;

    [JsonProperty("trigger")]
    public string Trigger { get; set; } = string.Empty;

    [JsonProperty("condicion")]
    public string? Condicion { get; set; }

    [JsonProperty("accion")]
    public string? Accion { get; set; }

    [JsonProperty("mensaje")]
    public string? Mensaje { get; set; }

    [JsonProperty("activa")]
    public bool Activa { get; set; } = true;
}
