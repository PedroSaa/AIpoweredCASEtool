using Newtonsoft.Json;

namespace AicaseCli.Models;

public class TechnologyConfig
{
    [JsonProperty("backend")]
    public string Backend { get; set; } = "dotnet8";

    [JsonProperty("frontend")]
    public string Frontend { get; set; } = "angular17";

    [JsonProperty("baseDatos")]
    public string Database { get; set; } = "sqlserver";
}
