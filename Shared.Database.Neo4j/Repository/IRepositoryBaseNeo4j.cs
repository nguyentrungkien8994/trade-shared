using Shared.Database.Neo4j.Requests;
using Shared.Database.Neo4j.Responses;

namespace Shared.Database.Neo4j.Repository
{
    public interface IRepositoryBaseNeo4j<T, TId>
    {
        Task<T?> GetAsync(TId id);
        Task<IEnumerable<T>> GetAllAsync();
        Task<object?> SearchNode(CypherQuery searchParam);
        //Task<object?> SearchNodeByRel();
        //Task<object?> SearchRelBetweenNodes();
        Task<T?> InsertAsync(T entity);
        Task<int> DeleteAsync(TId id);
        Task<T?> UpdateAsync(T entity);
    }
}
