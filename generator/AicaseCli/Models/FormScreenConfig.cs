using Newtonsoft.Json;

namespace AicaseCli.Models;

public class FormScreenConfig
{
    [JsonProperty("titulo")]
    public string? Titulo { get; set; }

    [JsonProperty("layout")]
    public string Layout { get; set; } = "simple";

    [JsonProperty("campos")]
    public List<string> Campos { get; set; } = new();

    [JsonProperty("secciones")]
    public List<FormSection> Secciones { get; set; } = new();
}
