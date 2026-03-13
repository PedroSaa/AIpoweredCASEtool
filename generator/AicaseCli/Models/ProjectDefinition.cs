using Newtonsoft.Json;

namespace AicaseCli.Models;

public class ProjectDefinition
{
    [JsonProperty("nombre")]
    public string Nombre { get; set; } = string.Empty;

    [JsonProperty("version")]
    public string Version { get; set; } = "1.0.0";

    [JsonProperty("descripcion")]
    public string? Descripcion { get; set; }

    [JsonProperty("tecnologias")]
    public TechnologyConfig? Tecnologias { get; set; }

    [JsonProperty("entidades")]
    public List<EntityDefinition> Entidades { get; set; } = new();

    [JsonProperty("configuracion")]
    public GlobalConfig? Configuracion { get; set; }

    /// <summary>Namespace base del proyecto backend, derivado de la configuración.</summary>
    public string Namespace => Configuracion?.NamespaceBackend ?? $"{Nombre}.Api";
}
