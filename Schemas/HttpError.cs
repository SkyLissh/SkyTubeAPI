using System.Text.Json.Serialization;

namespace SkyTube.Schemas;

public record HttpMessage([property: JsonPropertyName("message")] string Message);
