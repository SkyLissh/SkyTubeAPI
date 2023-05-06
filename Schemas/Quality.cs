using System.Text.Json.Serialization;

namespace SkyTube.Schemas;

// [JsonConverter(typeof(JsonEnumConverter))]
public enum Quality
{
  [JsonPropertyName("144p")]
  P144,
  [JsonPropertyName("240p")]
  P240,
  [JsonPropertyName("360p")]
  P360,
  [JsonPropertyName("480p")]
  P480,
  [JsonPropertyName("720p")]
  P720,
  [JsonPropertyName("720p60")]
  P720_60,
  [JsonPropertyName("1080p")]
  P1080,
  [JsonPropertyName("1080p60")]
  P1080_60,
  [JsonPropertyName("1440p")]
  P1440,
  [JsonPropertyName("1440p60")]
  P1440_60,
  [JsonPropertyName("2160p")]
  P2160,
  [JsonPropertyName("2160p60")]
  P2160_60,
  [JsonPropertyName("4320p")]
  P4320,
  [JsonPropertyName("4320p60")]
  P4320_60
}
