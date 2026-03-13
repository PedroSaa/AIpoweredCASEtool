using Newtonsoft.Json;

namespace AicaseCli.Models;

public class FormSection
{
    [JsonProperty("titulo")]
    public string Titulo { get; set; } = string.Empty;

    [JsonProperty("campos")]
    public List<string> Campos { get; set; } = new();

    [JsonProperty("columnas")]
    public int Columnas { get; set; } = 2;

    [JsonProperty("colapsable")]
    public bool Colapsable { get; set; }
}
