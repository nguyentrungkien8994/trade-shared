using KLib.Core.Database;
using Shared.Database.Neo4j.Requests;
using Shared.Database.Neo4j.Responses;

namespace Shared.Database.Neo4j.Repository;

public interface IRepositoryBaseNeo4j : IRepositoryBase
{
    Task<int> UpSertNodeAsync(IEnumerable<IDictionary<string,object>> nodes, string nodeName, string idKey = "id");
    Task<int> UpSertRelationshipAsync(IEnumerable<Relationship> rels, string fromKey = "id", string toKey = "id");
}
public interface IRepositoryBaseNeo4j<T, TId>
{
    Task<T?> GetAsync(TId id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<object?> SearchNode(CypherQuery searchParam);
    Task<T?> InsertAsync(T entity);
    Task<int> DeleteAsync(TId id);
    Task<T?> UpdateAsync(T entity);
    Task<int> UpSertNodeAsync(IEnumerable<object> nodes, string idKey = "id");
}
