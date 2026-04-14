using KLib.Core.Database.Entity;
using Newtonsoft.Json;
namespace Shared.Database.Neo4j.Entity;

public class EntityBase<TId> : IEntityKey<TId>
{
    public required TId entityId { get; set; }
    public long created_at { get; set; }
    public long updated_at { get; set; }
    public required string created_by { get; set; }
    public required string updated_by { get; set; }
}
