using Newtonsoft.Json;

namespace AicaseCli.Models;

public class ListScreenConfig
{
    [JsonProperty("titulo")]
    public string? Titulo { get; set; }

    [JsonProperty("columnas")]
    public List<string> Columnas { get; set; } = new();

    [JsonProperty("acciones")]
    public List<string> Acciones { get; set; } = new();

    [JsonProperty("filtros")]
    public List<string> Filtros { get; set; } = new();

    [JsonProperty("paginacion")]
    public PaginationConfig? Paginacion { get; set; }

    [JsonProperty("ordenamiento")]
    public string? Ordenamiento { get; set; }

    [JsonProperty("ordenAscendente")]
    public bool OrdenAscendente { get; set; } = true;
}
