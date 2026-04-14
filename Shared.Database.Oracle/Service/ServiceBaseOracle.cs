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
    public virtual Task<IEnumerable<IDictionary<string, object>>> GetAllObjectAsync(string tableName)
    {
        return _repositoryBase.GetAllObjectAsync(tableName);
    }

    public virtual Task<PagingObject<IDictionary<string, object>>> PagingObjectAsync(string tableName, int skip, int take, IDictionary<string, object>? filters = null, IEnumerable<(string field, bool desc)>? sort = null)
    {
        return _repositoryBase.PagingObjectAsync(tableName, skip, take, filters, sort);
    }

    public virtual Task<IEnumerable<IDictionary<string, object>>> SearchObjectAsync(string tableName, IDictionary<string, object>? filters = null, IEnumerable<(string field, bool desc)>? sort = null)
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

    public virtual Task<int> DeleteAsync(TId id)
    {
        return _repositoryBase.DeleteAsync(id);
    }

    public virtual Task<IEnumerable<T>> GetAllAsync()
    {
        return _repositoryBase.GetAllAsync();
    }

    public virtual Task<T?> GetAsync(TId id)
    {
        return _repositoryBase.GetAsync(id);
    }

    public virtual Task<int> InsertAsync(T entity)
    {
        throw new NotImplementedException();
    }

    public virtual Task<int> InsertBulkAsync(T[] entities)
    {
        throw new NotImplementedException();
    }

    public virtual Task<PagingObject<T>> PagingAsync(int skip, int take, IDictionary<string, object>? filters = null, IEnumerable<(string field, bool desc)>? sort = null)
    {
        throw new NotImplementedException();
    }

    public virtual Task<IEnumerable<T>> SearchAsync(IDictionary<string, object>? filters = null, IEnumerable<(string field, bool desc)>? sort = null)
    {
        throw new NotImplementedException();
    }

    public virtual Task<T?> SearchOneAsync(IDictionary<string, object>? filters = null, IEnumerable<(string field, bool desc)>? sort = null)
    {
        throw new NotImplementedException();
    }

    public virtual Task<int> UpdateAsync(T entity)
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
