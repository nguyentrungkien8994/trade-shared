using Shared.Database.Neo4j.Requests;

namespace Shared.Database.Neo4j.Builder;

public interface ICyperBuilder
{
    (string query, object parameters) BuildMergeRelationship(IEnumerable<Relationship> rels, string fromKey = "id", string toKey = "id");
}
