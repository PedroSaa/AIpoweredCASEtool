using Newtonsoft.Json;

namespace AicaseCli.Models;

public class FieldDefinition
{
    [JsonProperty("nombre")]
    public string Nombre { get; set; } = string.Empty;

    [JsonProperty("tipo")]
    public string Tipo { get; set; } = "string";

    [JsonProperty("largo")]
    public int? Largo { get; set; }

    [JsonProperty("precision")]
    public int? Precision { get; set; }

    [JsonProperty("escala")]
    public int? Escala { get; set; }

    [JsonProperty("requerido")]
    public bool Requerido { get; set; }

    [JsonProperty("unico")]
    public bool Unico { get; set; }

    [JsonProperty("esPK")]
    public bool EsPK { get; set; }

    [JsonProperty("autoIncremento")]
    public bool AutoIncremento { get; set; }

    [JsonProperty("valorPorDefecto")]
    public object? ValorPorDefecto { get; set; }

    [JsonProperty("validaciones")]
    public List<ValidationRule> Validaciones { get; set; } = new();

    [JsonProperty("input")]
    public InputConfig? Input { get; set; }

    [JsonProperty("relacion")]
    public RelationDefinition? Relacion { get; set; }

    /// <summary>Convierte el tipo JSON a tipo C#.</summary>
    public string ToCSharpType()
    {
        return Tipo switch
        {
            "string" => "string",
            "int" => "int",
            "long" => "long",
            "decimal" => "decimal",
            "double" => "double",
            "float" => "float",
            "date" or "datetime" => "DateTime",
            "bool" => "bool",
            "guid" => "Guid",
            "byte[]" => "byte[]",
            _ => "string"
        };
    }

    /// <summary>Convierte el tipo JSON a tipo TypeScript.</summary>
    public string ToTypeScriptType()
    {
        return Tipo switch
        {
            "string" or "guid" => "string",
            "int" or "long" or "decimal" or "double" or "float" => "number",
            "date" or "datetime" => "Date",
            "bool" => "boolean",
            "byte[]" => "string",
            _ => "string"
        };
    }

    /// <summary>Convierte el tipo JSON a tipo SQL Server.</summary>
    public string ToSqlType()
    {
        return Tipo switch
        {
            "string" => Largo.HasValue ? $"NVARCHAR({Largo})" : "NVARCHAR(MAX)",
            "int" => "INT",
            "long" => "BIGINT",
            "decimal" => $"DECIMAL({Precision ?? 18},{Escala ?? 2})",
            "double" => "FLOAT",
            "float" => "REAL",
            "date" => "DATE",
            "datetime" => "DATETIME2",
            "bool" => "BIT",
            "guid" => "UNIQUEIDENTIFIER",
            "byte[]" => "VARBINARY(MAX)",
            _ => "NVARCHAR(255)"
        };
    }
}
