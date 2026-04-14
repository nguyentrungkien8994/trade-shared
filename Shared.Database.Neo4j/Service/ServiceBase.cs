using KLib.Core.Database;
using KLib.Core.Database.Dto;
using KLib.Core.Database.Entity;
using Neo4j.Driver;
using Shared.Database.Neo4j.Repository;
using Shared.Database.Neo4j.Requests;
using Shared.Database.Neo4j.Responses;

namespace Shared.Database.Neo4j.Service;

public class ServiceBase : IServiceBase
{
    private readonly IRepositoryBase _repositoryBase;
    public ServiceBase(IRepositoryBase repositoryBase)
    {
        _repositoryBase = repositoryBase;
    }
    public Task<IEnumerable<IDictionary<string, object>>> GetAllObjectAsync(string nodeName)
    {
        return _repositoryBase.GetAllObjectAsync(nodeName);
    }

    public Task<PagingObject<IDictionary<string, object>>> PagingObjectAsync(string nodeName, int skip, int take, IDictionary<string, object>? filters = null, IEnumerable<(string field, bool desc)>? sort = null)
    {
        return _repositoryBase.PagingObjectAsync(nodeName, skip, take, filters, sort);
    }

    public Task<PagingObject<IDictionary<string, object>>> PagingObjectAsync(int skip, int take, IDictionary<string, object>? filters = null, IEnumerable<(string field, bool desc)>? sort = null)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<IDictionary<string, object>>> SearchObjectAsync(string objName, IDictionary<string, object>? filters = null, IEnumerable<(string field, bool desc)>? sort = null)
    {
        throw new NotImplementedException();
    }
}
public class ServiceBase<T, TId, IRepo> : IRepositoryBaseNeo4j<T, TId> where IRepo : IRepositoryBaseNeo4j<T, TId> where T : IEntityKey<TId>
{
    private readonly IRepo _repositoryBase;
    public ServiceBase(IRepo repositoryBase)
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

    public Task<int> UpSertNodeAsync(IEnumerable<object> upserts, string idKey = "id")
    {
        return _repositoryBase.UpSertNodeAsync(upserts, idKey);
    }

    public Task<int> UpSertRelationshipAsync(IEnumerable<Relationship> rels, string fromKey = "id", string toKey = "id")
    {
        throw new NotImplementedException();
    }
}
public class ServiceBase<T, TId> : ServiceBase<T, TId, IRepositoryBaseNeo4j<T, TId>>, IServiceBaseNeo4j<T, TId> where T : IEntityKey<TId>
{
    public ServiceBase(IRepositoryBaseNeo4j<T, TId> repositoryBase) : base(repositoryBase)
    {
    }
}
public class ServiceBase<T> : ServiceBase<T, Guid, IRepositoryBaseNeo4j<T, Guid>>, IServiceBaseNeo4j<T, Guid> where T : IEntityKey<Guid>
{
    public ServiceBase(IRepositoryBaseNeo4j<T, Guid> repositoryBase) : base(repositoryBase)
    {
    }
}
