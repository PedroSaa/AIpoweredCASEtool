using Newtonsoft.Json;

namespace AicaseCli.Models;

public class ScreenConfig
{
    [JsonProperty("listado")]
    public ListScreenConfig? Listado { get; set; }

    [JsonProperty("formulario")]
    public FormScreenConfig? Formulario { get; set; }
}
