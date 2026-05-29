using System.Text.Json;
using System.Text.Json.Serialization;
using SemVersion;

namespace MultiRPC.Converters;

public class VersionJsonConverter : JsonConverter<SemanticVersion>
{
    public override SemanticVersion? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            var versionS = reader.GetString();
            if (string.IsNullOrWhiteSpace(versionS))
            {
                throw new JsonException("We was given an empty string when reading the version!");
            }

            //This allows us to use Version strings in a way that work for us
            var lastDot = versionS!.LastIndexOf('.');
            if (lastDot != -1 && versionS.Count(x => x == '.') == 3)
            {
                versionS = versionS[..lastDot] + '-' + versionS[(lastDot + 1)..];
            }

            if (SemanticVersion.TryParse(versionS, out var version))
            {
                return version;
            }
            throw new JsonException("Wasn't able to make an SemanticVersion out of the string given! (" + versionS + ")");
        }
        
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException("We haven't been given an old styled version!");
        }
        reader.Read();

        //We never ended up with a version that contained the last two
        int major = 0, minor = 0, build = 0, revision = 0;//, majorRevision, minorRevision;
        while (reader.TokenType != JsonTokenType.EndObject)
        {
            var propName = reader.GetString();
            reader.Read();
            var val = reader.GetInt32();
            switch (propName)
            {
                case "Major":
                    major = val;
                    break;
                case "Minor":
                    minor = val;
                    break;
                case "Build":
                    build = val;
                    break;
                case "Revision":
                    revision = val;
                    break;
                /*case "MajorRevision":
                    majorRevision = val;
                    break;
                case "MinorRevision":
                    minorRevision = val;
                    break;*/
            }
            reader.Read();
        }
        return new SemanticVersion(major, minor, build, revision.ToString());
    }

    public override void Write(Utf8JsonWriter writer, SemanticVersion value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }
}