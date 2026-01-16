using MongoDB.Bson;

namespace Shared.MongoDB.Entity;

public class EntityBase : IEntityBase
{
    public ObjectId id { get; set; }
    public long created_at { get; set; }
    public long updated_at { get; set; }
    public required string created_by { get; set; }
    public required string updated_by { get; set; }
}
