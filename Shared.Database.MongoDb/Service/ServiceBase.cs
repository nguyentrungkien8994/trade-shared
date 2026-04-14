using KLib.Core.Database;
using KLib.Core.Database.Dto;
using KLib.Core.Database.Entity;
using System.Linq.Expressions;
using System.Security.Cryptography;

namespace Shared.Database.MongoDb.Service;

public class ServiceBase<T, TId, IRepo> : IServiceBase<T, TId, IRepo> where T : IEntityKey<TId> where IRepo : IRepositoryBase<T, TId>
{
    private readonly IRepo _repositoryBase;

    public ServiceBase(IRepo repositoryBase)
    {
        _repositoryBase = repositoryBase;
    }

    public IRepo Repository => _repositoryBase;

    public Task<int> DeleteAsync(TId id)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<T>> GetAllAsync()
    {
        throw new NotImplementedException();
    }

    public Task<T?> GetAsync(TId id)
    {
        throw new NotImplementedException();
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
public class ServiceBase<T, TId> : ServiceBase<T, TId, IRepositoryBase<T, TId>>, IServiceBase<T, TId> where T : IEntityKey<TId>
{
    public ServiceBase(IRepositoryBase<T, TId> repositoryBase) : base(repositoryBase)
    {
    }
}
public class ServiceBase<T> : ServiceBase<T, Guid, IRepositoryBase<T, Guid>>, IServiceBase<T, Guid> where T : IEntityKey<Guid>
{
    public ServiceBase(IRepositoryBase<T, Guid> repositoryBase) : base(repositoryBase)
    {
    }
}
