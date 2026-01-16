using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Shared.MongoDB.Entity;

public interface IEntityBase
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    ObjectId id { get; }
    string created_by { get; }
    string updated_by { get; }
    long created_at { get; }
    long updated_at { get; }
}
