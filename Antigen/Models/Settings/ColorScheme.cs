using System.Text.Json.Serialization;

namespace Antigen.Models.Settings;

[JsonConverter(typeof(JsonStringEnumConverter<ColorScheme>))]
public enum ColorScheme
{
    Antigen,
    Flat,
}
