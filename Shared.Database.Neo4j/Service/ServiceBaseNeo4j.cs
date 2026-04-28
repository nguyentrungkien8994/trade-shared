using KLib.Core.Database.Dto;
using KLib.Core.Database.Entity;
using Shared.Database.Neo4j.Repository;
using Shared.Database.Neo4j.Requests;

namespace Shared.Database.Neo4j.Service;

public class ServiceBaseNeo4j : IServiceBaseNeo4j
{
    private readonly IRepositoryBaseNeo4j _repositoryBase;
    public ServiceBaseNeo4j(IRepositoryBaseNeo4j repositoryBase)
    {
        _repositoryBase = repositoryBase;
    }
    public virtual Task<IEnumerable<IDictionary<string, object>>> GetAllObjectAsync(string nodeName)
    {
        return _repositoryBase.GetAllObjectAsync(nodeName);
    }

    public virtual Task<PagingObject<IDictionary<string, object>>> PagingObjectAsync(string nodeName, int skip, int take, IDictionary<string, object>? filters = null, IEnumerable<(string field, bool desc)>? sort = null)
    {
        return _repositoryBase.PagingObjectAsync(nodeName, skip, take, filters, sort);
    }

    public virtual Task<IEnumerable<IDictionary<string, object>>> SearchObjectAsync(string objName, IDictionary<string, object>? filters = null, IEnumerable<(string field, bool desc)>? sort = null)
    {
        return _repositoryBase.SearchObjectAsync(objName, filters, sort);
    }

    public virtual Task<int> UpSertNodeAsync(IEnumerable<IDictionary<string, object>> nodes, string nodeName, string idKey = "id")
    {
        return _repositoryBase.UpSertNodeAsync(nodes, nodeName, idKey);
    }

    public virtual Task<int> UpSertRelationshipAsync(IEnumerable<Relationship> rels, string fromKey = "id", string toKey = "id")
    {
        return _repositoryBase.UpSertRelationshipAsync(rels, fromKey, toKey);
    }
    public virtual Task<object?> SearchNodeAsync(SearchParam searchParam)
    {
        return _repositoryBase.SearchNodeAsync(searchParam);
    }

    public Task<object?> SearchNodeByCypherRawAsync(string cypher)
    {
        return _repositoryBase.SearchNodeByCypherRawAsync(cypher);
    }
}
public class ServiceBaseNeo4j<T, TId, IRepo> : IRepositoryBaseNeo4j<T, TId> where IRepo : IRepositoryBaseNeo4j<T, TId> where T : IEntityKey<TId>
{
    private readonly IRepo _repositoryBase;
    public ServiceBaseNeo4j(IRepo repositoryBase)
    {
        _repositoryBase = repositoryBase;
    }
    public virtual Task<int> DeleteAsync(TId id)
    {
        return _repositoryBase.DeleteAsync(id);
    }

    public virtual Task<IEnumerable<T>> GetAllAsync()
    {
        return _repositoryBase.GetAllAsync();
    }

    public virtual Task<T> GetAsync(TId id)
    {
        return _repositoryBase.GetAsync(id);
    }

    public virtual Task<T> InsertAsync(T entity)
    {
        return _repositoryBase.InsertAsync(entity);
    }

    public virtual Task<T> UpdateAsync(T entity)
    {
        return _repositoryBase.UpdateAsync(entity);
    }

}
public class ServiceBaseNeo4j<T, TId> : ServiceBaseNeo4j<T, TId, IRepositoryBaseNeo4j<T, TId>>, IServiceBaseNeo4j<T, TId> where T : IEntityKey<TId>
{
    public ServiceBaseNeo4j(IRepositoryBaseNeo4j<T, TId> repositoryBase) : base(repositoryBase)
    {
    }
}
public class ServiceBaseNeo4j<T> : ServiceBaseNeo4j<T, Guid, IRepositoryBaseNeo4j<T, Guid>>, IServiceBaseNeo4j<T, Guid> where T : IEntityKey<Guid>
{
    public ServiceBaseNeo4j(IRepositoryBaseNeo4j<T, Guid> repositoryBase) : base(repositoryBase)
    {
    }
}
