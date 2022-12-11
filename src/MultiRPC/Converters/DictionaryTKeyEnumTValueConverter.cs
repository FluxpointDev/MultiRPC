using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MultiRPC.Converters;

/*From https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/converters-how-to?pivots=dotnet-7-0#sample-factory-pattern-converter
  with small changes*/
public class DictionaryTKeyEnumTValueConverter<TKey, TValue> : JsonConverterFactory
    where TKey : struct, Enum
{
    public override bool CanConvert(Type typeToConvert)
    {
        if (!typeToConvert.IsGenericType)
        {
            return false;
        }

        if (typeToConvert.GetGenericTypeDefinition() != typeof(Dictionary<,>))
        {
            return false;
        }

        return typeToConvert.GetGenericArguments()[0] == typeof(TKey)
            && typeToConvert.GetGenericArguments()[1] == typeof(TValue);
    }

    public override JsonConverter CreateConverter(
        Type type,
        JsonSerializerOptions options) =>
        new DictionaryEnumConverterInner(options);

    private class DictionaryEnumConverterInner :
        JsonConverter<Dictionary<TKey, TValue>>
    {
        private readonly JsonConverter<TValue> _valueConverter;
        private readonly Type _keyType;
        private readonly Type _valueType;

        public DictionaryEnumConverterInner(JsonSerializerOptions options)
        {
            // For performance, use the existing converter.
            _valueConverter = (JsonConverter<TValue>)options
                .GetTypeInfo(typeof(TValue)).Converter;

            // Cache the key and value types.
            _keyType = typeof(TKey);
            _valueType = typeof(TValue);
        }

        public override Dictionary<TKey, TValue> Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException();
            }

            var dictionary = new Dictionary<TKey, TValue>();

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                {
                    return dictionary;
                }

                // Get the key.
                if (reader.TokenType != JsonTokenType.PropertyName)
                {
                    throw new JsonException();
                }

                string? propertyName = reader.GetString();

                // For performance, parse with ignoreCase:false first.
                if (!Enum.TryParse(propertyName, ignoreCase: false, out TKey key) &&
                    !Enum.TryParse(propertyName, ignoreCase: true, out key))
                {
                    throw new JsonException(
                        $"Unable to convert \"{propertyName}\" to Enum \"{_keyType}\".");
                }

                // Get the value.
                reader.Read();
                TValue value = _valueConverter.Read(ref reader, _valueType, options)!;

                // Add to dictionary.
                dictionary.Add(key, value);
            }

            throw new JsonException();
        }

        public override void Write(
            Utf8JsonWriter writer,
            Dictionary<TKey, TValue> dictionary,
            JsonSerializerOptions options)
        {
            writer.WriteStartObject();

            foreach ((TKey key, TValue value) in dictionary)
            {
                var propertyName = key.ToString();
                writer.WritePropertyName
                    (options.PropertyNamingPolicy?.ConvertName(propertyName) ?? propertyName);

                _valueConverter.Write(writer, value, options);
            }

            writer.WriteEndObject();
        }
    }
}