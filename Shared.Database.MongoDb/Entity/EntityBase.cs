using KLib.Core.Database.Entity;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
namespace Shared.Database.MongoDb.Entity;

public class EntityBase : IEntityKey<ObjectId>
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    [JsonConverter(typeof(ObjectIdJsonConverter))]
    public ObjectId entityId { get; set; }
    public long created_at { get; set; }
    public long updated_at { get; set; }
    public required string created_by { get; set; }
    public required string updated_by { get; set; }
}
