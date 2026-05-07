using KLib.Core.Database;
using Shared.Database.Neo4j.Requests;
using Shared.Database.Neo4j.Responses;

namespace Shared.Database.Neo4j.Repository;

public interface IRepositoryBaseNeo4j : IRepositoryBase
{
    Task<int> UpSertNodeAsync(IEnumerable<IDictionary<string,object>> nodes, string nodeName, string idKey = "id");
    Task<int> UpSertRelationshipAsync(IEnumerable<Relationship> rels, string fromKey = "id", string toKey = "id");
    Task<object?> SearchNodeAsync(SearchParam searchParam);
    Task<object?> SearchNodeByCypherRawAsync<T>(string cypher);
}
public interface IRepositoryBaseNeo4j<T, TId>
{
    Task<T?> GetAsync(TId id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<T?> InsertAsync(T entity);
    Task<int> DeleteAsync(TId id);
    Task<T?> UpdateAsync(T entity);
}
