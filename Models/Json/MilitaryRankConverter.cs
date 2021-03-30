using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace TableTennis.Models.Json
{
    public sealed class MilitaryRankConverter : JsonConverter<MilitaryRank>
    {
        public override MilitaryRank Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException();
            }
            int rankId = default;
            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.PropertyName)
                {
                    string propertyName = reader.GetString() ?? string.Empty;
                    if (propertyName != nameof(MilitaryRank.Id)) throw new JsonException();
                    reader.Read();
                    rankId = reader.GetInt32();
                }
                if (reader.TokenType == JsonTokenType.EndObject)
                    return MilitaryRank.GetById(rankId);
            }
            throw new JsonException();

        }

        public override void Write(Utf8JsonWriter writer, MilitaryRank value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WriteNumber(nameof(MilitaryRank.Id), value.Id);
            writer.WriteEndObject();
        }
    }
}