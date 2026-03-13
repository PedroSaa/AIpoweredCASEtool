using Newtonsoft.Json;

namespace AicaseCli.Models;

public class InputConfig
{
    [JsonProperty("tipo")]
    public string Tipo { get; set; } = "text";

    [JsonProperty("label")]
    public string? Label { get; set; }

    [JsonProperty("placeholder")]
    public string? Placeholder { get; set; }

    [JsonProperty("fuenteDatos")]
    public string? FuenteDatos { get; set; }

    [JsonProperty("campoValor")]
    public string? CampoValor { get; set; }

    [JsonProperty("campoTexto")]
    public string? CampoTexto { get; set; }

    [JsonProperty("soloLectura")]
    public bool SoloLectura { get; set; }

    [JsonProperty("ocultarEnFormulario")]
    public bool OcultarEnFormulario { get; set; }

    [JsonProperty("ocultarEnListado")]
    public bool OcultarEnListado { get; set; }
}
