using System.Text.Json.Serialization;

namespace SkyTube.Extensions;

public static class EnumExtensions
{
  public static string? GetName(this Enum value)
  {
    var attr = value.GetType()?.GetField(value.ToString())
        ?.GetCustomAttributes(typeof(JsonPropertyNameAttribute), false);

    return ((JsonPropertyNameAttribute?)attr?[0])?.Name;
  }
}
