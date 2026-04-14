using KLib.Core.Database;
using KLib.Core.Database.Dto;
using KLib.Core.Database.Entity;
using Shared.Database.Oracle.Repository;

namespace Shared.Database.Oracle.Service;

public class ServiceBase : IServiceBaseOracle
{
    private readonly IRepositoryBase _repositoryBase;
    public ServiceBase(IRepositoryBase repositoryBase)
    {
        _repositoryBase = repositoryBase;
    }
    public Task<IEnumerable<IDictionary<string, object>>> GetAllObjectAsync(string tableName)
    {
        return _repositoryBase.GetAllObjectAsync(tableName);
    }

    public Task<PagingObject<IDictionary<string, object>>> PagingObjectAsync(string tableName, int skip, int take, IDictionary<string, object>? filters = null, IEnumerable<(string field, bool desc)>? sort = null)
    {
        return _repositoryBase.PagingObjectAsync(tableName, skip, take, filters, sort);
    }

    public Task<IEnumerable<IDictionary<string, object>>> SearchObjectAsync(IDictionary<string, object>? filters = null, IEnumerable<(string field, bool desc)>? sort = null)
    {
        return _repositoryBase.SearchObjectAsync(filters, sort);
    }

    public Task<IEnumerable<IDictionary<string, object>>> SearchObjectAsync(string objName, IDictionary<string, object>? filters = null, IEnumerable<(string field, bool desc)>? sort = null)
    {
        throw new NotImplementedException();
    }
}
public class ServiceBase<T, TId, IRepo> : IServiceBaseOracle<T, TId, IRepo> where T : IEntityKey<TId> where IRepo : IRepositoryBaseOracle<T, TId>
{
    private readonly IRepo _repositoryBase;

    public ServiceBase(IRepo repositoryBase)
    {
        _repositoryBase = repositoryBase;
    }

    public IRepo Repository => _repositoryBase;

    public Task<int> DeleteAsync(TId id)
    {
        return _repositoryBase.DeleteAsync(id);
    }

    public Task<IEnumerable<T>> GetAllAsync()
    {
        return _repositoryBase.GetAllAsync();
    }

    public Task<T?> GetAsync(TId id)
    {
        return _repositoryBase.GetAsync(id);
    }

    public Task<int> InsertAsync(T entity)
    {
        throw new NotImplementedException();
    }

    public Task<int> InsertBulkAsync(T[] entities)
    {
        throw new NotImplementedException();
    }

    public Task<PagingObject<T>> PagingAsync(int skip, int take, IDictionary<string, object>? filters = null, IEnumerable<(string field, bool desc)>? sort = null)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<T>> SearchAsync(IDictionary<string, object>? filters = null, IEnumerable<(string field, bool desc)>? sort = null)
    {
        throw new NotImplementedException();
    }

    public Task<T?> SearchOneAsync(IDictionary<string, object>? filters = null, IEnumerable<(string field, bool desc)>? sort = null)
    {
        throw new NotImplementedException();
    }

    public Task<int> UpdateAsync(T entity)
    {
        throw new NotImplementedException();
    }
}
public class ServiceBase<T, TId> : ServiceBase<T, TId, IRepositoryBaseOracle<T, TId>>, IServiceBaseOracle<T, TId> where T : IEntityKey<TId>
{
    public ServiceBase(IRepositoryBaseOracle<T, TId> repositoryBase) : base(repositoryBase)
    {
    }
}
public class ServiceBase<T> : ServiceBase<T, Guid, IRepositoryBaseOracle<T, Guid>>, IServiceBaseOracle<T, Guid> where T : IEntityKey<Guid>
{
    public ServiceBase(IRepositoryBaseOracle<T, Guid> repositoryBase) : base(repositoryBase)
    {
    }
}
