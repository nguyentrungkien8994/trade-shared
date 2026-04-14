using KLib.Core.Database.Dto;
using KLib.Core.Database.Entity;
using Shared.Database.Oracle.Repository;

namespace Shared.Database.Oracle.Service;

public class ServiceBaseOracle : IServiceBaseOracle
{
    private readonly IRepositoryBaseOracle _repositoryBase;
    public ServiceBaseOracle(IRepositoryBaseOracle repositoryBase)
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

    public Task<IEnumerable<IDictionary<string, object>>> SearchObjectAsync(string tableName, IDictionary<string, object>? filters = null, IEnumerable<(string field, bool desc)>? sort = null)
    {
        return _repositoryBase.SearchObjectAsync(tableName, filters, sort);
    }

}
public class ServiceBaseOracle<T, TId, IRepo> : IServiceBaseOracle<T, TId, IRepo> where T : IEntityKey<TId> where IRepo : IRepositoryBaseOracle<T, TId>
{
    private readonly IRepo _repositoryBase;

    public ServiceBaseOracle(IRepo repositoryBase)
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
public class ServiceBaseOracle<T, TId> : ServiceBaseOracle<T, TId, IRepositoryBaseOracle<T, TId>>, IServiceBaseOracle<T, TId> where T : IEntityKey<TId>
{
    public ServiceBaseOracle(IRepositoryBaseOracle<T, TId> repositoryBase) : base(repositoryBase)
    {
    }
}
public class ServiceBaseOracle<T> : ServiceBaseOracle<T, Guid, IRepositoryBaseOracle<T, Guid>>, IServiceBaseOracle<T, Guid> where T : IEntityKey<Guid>
{
    public ServiceBaseOracle(IRepositoryBaseOracle<T, Guid> repositoryBase) : base(repositoryBase)
    {
    }
}
