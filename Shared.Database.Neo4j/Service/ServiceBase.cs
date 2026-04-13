using KLib.Core.Database;
using KLib.Core.Database.Entity;
using Neo4j.Driver;
using Shared.Database.Neo4j.Repository;
using Shared.Database.Neo4j.Requests;
using Shared.Database.Neo4j.Responses;

namespace Shared.Database.Neo4j.Service;

public class ServiceBase<T, TId, IRepo> : IRepositoryBaseNeo4j<T, TId> where IRepo : IRepositoryBaseNeo4j<T, TId> where T : IEntityBase<TId>
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
        return _repositoryBase.UpSertNodeAsync(upserts,idKey);
    }

    public Task<int> UpSertRelationshipAsync(IEnumerable<Relationship> rels, string fromKey = "id", string toKey = "id")
    {
        return _repositoryBase.UpSertRelationshipAsync(rels, fromKey, toKey);
    }
}
public class ServiceBase<T, TId> : ServiceBase<T, TId, IRepositoryBaseNeo4j<T, TId>>, IServiceBaseNeo4j<T, TId> where T : IEntityBase<TId>
{
    public ServiceBase(IRepositoryBaseNeo4j<T, TId> repositoryBase) : base(repositoryBase)
    {
    }
}
public class ServiceBase<T> : ServiceBase<T, Guid, IRepositoryBaseNeo4j<T, Guid>>, IServiceBaseNeo4j<T, Guid> where T : IEntityBase<Guid>
{
    public ServiceBase(IRepositoryBaseNeo4j<T, Guid> repositoryBase) : base(repositoryBase)
    {
    }
}
