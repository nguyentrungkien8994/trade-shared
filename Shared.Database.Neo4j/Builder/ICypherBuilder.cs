using Shared.Database.Neo4j.Requests;

namespace Shared.Database.Neo4j.Builder;

public interface ICypherBuilder
{
    string BuildGetAll(string entityName);
    (string query, object parameters) BuildCreate<T>(T entity);
    (string query, object parameters) BuildDelete<T>(object id);
    (string query, object parameters) BuildGet<T>(object id);
    (string query, object parameters) BuildUpdate<T>(T entity);
    (string query, object parameters) BuildMergeNode<T>(IEnumerable<object> nodes, string idKey = "id");
    (string query, object parameters) BuildMergeNode(IEnumerable<IDictionary<string, object>> nodes, string nodeName, string idKey = "id");
    (string query, object parameters) BuildMergeRelationship(IEnumerable<Relationship> rels, string fromKey = "id", string toKey = "id");
}
