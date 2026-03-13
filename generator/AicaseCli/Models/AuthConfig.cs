using Newtonsoft.Json;

namespace AicaseCli.Models;

public class AuthConfig
{
    [JsonProperty("tipo")]
    public string Tipo { get; set; } = "jwt";

    [JsonProperty("key")]
    public string? Key { get; set; }

    [JsonProperty("issuer")]
    public string? Issuer { get; set; }

    [JsonProperty("audience")]
    public string? Audience { get; set; }

    [JsonProperty("expireMinutes")]
    public int ExpireMinutes { get; set; } = 480;
}
