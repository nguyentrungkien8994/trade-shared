using KLib.Core.Database.Dto;
using KLib.Core.Database.Entity;
using Shared.Database.Neo4j.Repository;
using Shared.Database.Neo4j.Requests;
using Shared.Database.Neo4j.Responses;

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

    public Task<string> GetAllObjectJsonAsync(string objName)
    {
        throw new NotImplementedException();
    }

    public virtual Task<PagingObject<IDictionary<string, object>>> PagingObjectAsync(string nodeName, int skip, int take, IDictionary<string, object>? filters = null, IEnumerable<(string field, bool desc)>? sort = null)
    {
        return _repositoryBase.PagingObjectAsync(nodeName, skip, take, filters, sort);
    }

    public Task<PagingObject<string>> PagingObjectJsonAsync(string objName, int skip, int take, IDictionary<string, object>? filters = null, IEnumerable<(string field, bool desc)>? sort = null)
    {
        throw new NotImplementedException();
    }

    public virtual Task<IEnumerable<IDictionary<string, object>>> SearchObjectAsync(string objName, IDictionary<string, object>? filters = null, IEnumerable<(string field, bool desc)>? sort = null)
    {
        return _repositoryBase.SearchObjectAsync(objName, filters, sort);
    }

    public Task<string> SearchObjectJsonAsync(string objName, IDictionary<string, object>? filters = null, IEnumerable<(string field, bool desc)>? sort = null)
    {
        throw new NotImplementedException();
    }

    public Task<int> UpSertNodeAsync(IEnumerable<IDictionary<string, object>> nodes, string nodeName, string idKey = "id")
    {
        return _repositoryBase.UpSertNodeAsync(nodes, nodeName, idKey);
    }

    public Task<int> UpSertRelationshipAsync(IEnumerable<Relationship> rels, string fromKey = "id", string toKey = "id")
    {
        return _repositoryBase.UpSertRelationshipAsync(rels, fromKey, toKey);
    }
}
public class ServiceBaseNeo4j<T, TId, IRepo> : IRepositoryBaseNeo4j<T, TId> where IRepo : IRepositoryBaseNeo4j<T, TId> where T : IEntityKey<TId>
{
    private readonly IRepo _repositoryBase;
    public ServiceBaseNeo4j(IRepo repositoryBase)
    {
        _repositoryBase = repositoryBase;
    }
    public Task<int> DeleteAsync(TId id)
    {
        return _repositoryBase.DeleteAsync(id);
    }

    public Task<IEnumerable<T>> GetAllAsync()
    {
        return _repositoryBase.GetAllAsync();
    }

    public Task<T> GetAsync(TId id)
    {
        return _repositoryBase.GetAsync(id);
    }

    public Task<T> InsertAsync(T entity)
    {
        return _repositoryBase.InsertAsync(entity);
    }

    public Task<object?> SearchNode(CypherQuery searchParam)
    {
        return _repositoryBase.SearchNode(searchParam);
    }

    public Task<T> UpdateAsync(T entity)
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
