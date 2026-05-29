using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using Avalonia.Media;

namespace MultiRPC.Converters;

public class ColourJsonConverter : JsonConverter<Color>
{
    public override Color Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var s = reader.GetString();
        if (string.IsNullOrWhiteSpace(s))
        {
            throw new JsonException("We should of been given an string when reading the colour!");
        }
        
        return Color.FromArgb(byte.Parse(s[1..3], NumberStyles.HexNumber), byte.Parse(s[3..5], NumberStyles.HexNumber), byte.Parse(s[5..7], NumberStyles.HexNumber), byte.Parse(s[7..9], NumberStyles.HexNumber));
    }

    public override void Write(Utf8JsonWriter writer, Color value, JsonSerializerOptions options)
    {
        writer.WriteStringValue("#" + value.ToUint32().ToString("x8"));
    }
}