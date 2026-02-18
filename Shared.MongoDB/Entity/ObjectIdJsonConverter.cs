using MongoDB.Bson;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
namespace Shared.MongoDB.Entity;

public class ObjectIdJsonConverter : JsonConverter<ObjectId>
{
    public override ObjectId ReadJson(
        JsonReader reader,
        Type objectType,
        ObjectId existingValue,
        bool hasExistingValue,
        JsonSerializer serializer)
    {
        // id không tồn tại
        if (reader.TokenType == JsonToken.Null)
            return ObjectId.Empty;

        // id là object { timestamp, creationTime }
        if (reader.TokenType == JsonToken.StartObject)
        {
            var obj = JObject.Load(reader);

            var tsToken = obj["timestamp"];
            if (tsToken == null)
                return ObjectId.Empty;

            var seconds = tsToken.Value<int>();

            // Dựng ObjectId từ timestamp
            return ObjectId.GenerateNewId(
                DateTimeOffset.FromUnixTimeSeconds(seconds).UtcDateTime
            );
        }

        // fallback nếu id là string
        if (reader.TokenType == JsonToken.String &&
            ObjectId.TryParse(reader.Value?.ToString(), out var oid))
        {
            return oid;
        }

        // các case khác → bỏ qua
        reader.Skip();
        return ObjectId.Empty;
    }

    public override void WriteJson(JsonWriter writer, ObjectId value, JsonSerializer serializer)
    {
        writer.WriteValue(value.ToString());
    }
}
