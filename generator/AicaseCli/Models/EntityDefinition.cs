using Newtonsoft.Json;

namespace AicaseCli.Models;

public class EntityDefinition
{
    [JsonProperty("nombre")]
    public string Nombre { get; set; } = string.Empty;

    [JsonProperty("tabla")]
    public string Tabla { get; set; } = string.Empty;

    [JsonProperty("descripcion")]
    public string? Descripcion { get; set; }

    [JsonProperty("campos")]
    public List<FieldDefinition> Campos { get; set; } = new();

    [JsonProperty("relaciones")]
    public List<RelationDefinition> Relaciones { get; set; } = new();

    [JsonProperty("reglasNegocio")]
    public List<BusinessRule> ReglasNegocio { get; set; } = new();

    [JsonProperty("pantallas")]
    public ScreenConfig? Pantallas { get; set; }

    [JsonProperty("api")]
    public ApiConfig? Api { get; set; }

    /// <summary>Devuelve el campo marcado como clave primaria, si existe.</summary>
    public FieldDefinition? PrimaryKey => Campos.FirstOrDefault(c => c.EsPK);

    /// <summary>Devuelve todos los campos que no son clave primaria.</summary>
    public IEnumerable<FieldDefinition> NonPkFields => Campos.Where(c => !c.EsPK);
}
