using MongoDB.Bson;
using Newtonsoft.Json;
namespace Shared.MongoDB.Entity;

public class EntityBase : IEntityBase
{
    [JsonConverter(typeof(ObjectIdJsonConverter))]
    public ObjectId id { get; set; }
    public long created_at { get; set; }
    public long updated_at { get; set; }
    public required string created_by { get; set; }
    public required string updated_by { get; set; }
}
