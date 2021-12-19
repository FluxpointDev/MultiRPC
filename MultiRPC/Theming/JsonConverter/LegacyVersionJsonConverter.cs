using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MultiRPC.Theming.JsonConverter;

public class LegacyVersionJsonConverter : JsonConverter<Version>
{
    public override Version? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new Exception();
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
        return new Version(major, minor, build, revision);
    }

    public override void Write(Utf8JsonWriter writer, Version value, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }
}