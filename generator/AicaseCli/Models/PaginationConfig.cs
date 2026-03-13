using Newtonsoft.Json;

namespace AicaseCli.Models;

public class PaginationConfig
{
    [JsonProperty("registrosPorPagina")]
    public int RegistrosPorPagina { get; set; } = 10;

    [JsonProperty("mostrarTodos")]
    public bool MostrarTodos { get; set; }

    [JsonProperty("opcionesTamano")]
    public List<int> OpcionesTamano { get; set; } = new() { 10, 25, 50, 100 };
}
