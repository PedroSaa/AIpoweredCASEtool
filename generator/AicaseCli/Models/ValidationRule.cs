using Newtonsoft.Json;

namespace AicaseCli.Models;

public class ValidationRule
{
    [JsonProperty("tipo")]
    public string Tipo { get; set; } = string.Empty;

    [JsonProperty("valor")]
    public object? Valor { get; set; }

    [JsonProperty("mensaje")]
    public string? Mensaje { get; set; }
}
