using Core.Database;
using Core.Database.Dto;
using Core.Database.Entity;
using Shared.Database.Oracle.Repository;
using System.Linq.Expressions;

namespace Shared.Database.Oracle.Service;

public class ServiceBase<T, TId, IRepo> : IServiceBaseOracle<T, TId, IRepo> where T : IEntityBase<TId> where IRepo : IRepositoryBaseOracle<T, TId>
{
    private readonly IRepo _repositoryBase;

    public ServiceBase(IRepo repositoryBase)
    {
        _repositoryBase = repositoryBase;
    }

    public IRepo Repository => _repositoryBase;


    public Task<long> CountAsync(Expression<Func<T, bool>> expression)
    {
        return _repositoryBase.CountAsync(expression);
    }

    public Task<int> DeleteAsync(TId id)
    {
        return _repositoryBase.DeleteAsync(id);
    }

    public Task<int> DeleteRangeAsync(T[] entities)
    {
        return _repositoryBase.DeleteRangeAsync(entities);
    }

    public Task<List<T>> GetAllAsync()
    {
        return _repositoryBase.GetAllAsync();
    }

    public Task<T> GetAsync(TId id)
    {
        return _repositoryBase.GetAsync(id);
    }

    public Task<PagingObject<T>> GetPaging(int skip, int take, IDictionary<string, object>? filters = null, IEnumerable<(string field, bool desc)>? sort = null)
    {
        return _repositoryBase.GetPaging(skip,take,filters,sort);
    }

    public Task<int> InsertAsync(T entity)
    {
        return (_repositoryBase.InsertAsync(entity));
    }

    public Task<int> InsertRangeAsync(T[] entities)
    {
        return _repositoryBase.InsertRangeAsync(entities);
    }

    public Task<IEnumerable<T>> Search(IDictionary<string, object>? filters = null)
    {
        return _repositoryBase.Search(filters);
    }

    public Task<List<T>> SearchAsync(Expression<Func<T, bool>> expression)
    {
        return _repositoryBase.SearchAsync(expression);
    }

    public Task<List<T>> SearchBySqlRawAsync(string sqlText, params object[] parameters)
    {
        return _repositoryBase.SearchBySqlRawAsync<T>(sqlText, parameters);
    }

    public Task<List<TResult>> SearchBySqlRawAsync<TResult>(string sqlText, params object[] parameters)
    {
        throw new NotImplementedException();
    }

    public Task<T> SearchOnceBySqlRawAsync(string sqlText, params object[] parameters)
    {
        throw new NotImplementedException();
    }

    public Task<TResult?> SearchOnceBySqlRawAsync<TResult>(string sqlText, params object[] parameters)
    {
        throw new NotImplementedException();
    }

    public Task<T> SearchOneAsync(Expression<Func<T, bool>> expression)
    {
        return _repositoryBase.SearchOneAsync(expression);
    }

    public Task<int> UpdateAsync(T entity)
    {
        return _repositoryBase.UpdateAsync(entity);
    }

    public Task<int> UpdateRangeAsync(T[] entities)
    {
        return _repositoryBase.UpdateRangeAsync(entities);
    }
}
public class ServiceBase<T, TId> : ServiceBase<T, TId, IRepositoryBaseOracle<T, TId>>, IServiceBaseOracle<T, TId> where T : IEntityBase<TId>
{
    public ServiceBase(IRepositoryBaseOracle<T, TId> repositoryBase) : base(repositoryBase)
    {
    }
}
public class ServiceBase<T> : ServiceBase<T, Guid, IRepositoryBaseOracle<T, Guid>>, IServiceBaseOracle<T, Guid> where T : IEntityBase<Guid>
{
    public ServiceBase(IRepositoryBaseOracle<T, Guid> repositoryBase) : base(repositoryBase)
    {
    }
}
