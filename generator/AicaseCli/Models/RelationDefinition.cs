using Newtonsoft.Json;

namespace AicaseCli.Models;

public class RelationDefinition
{
    [JsonProperty("tipo")]
    public string Tipo { get; set; } = string.Empty;

    [JsonProperty("entidad")]
    public string Entidad { get; set; } = string.Empty;

    [JsonProperty("campo")]
    public string? Campo { get; set; }

    [JsonProperty("nombrePropiedad")]
    public string? NombrePropiedad { get; set; }

    [JsonProperty("cascada")]
    public bool Cascada { get; set; }
}
