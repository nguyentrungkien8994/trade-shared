using System.Linq.Expressions;
using MongoDB.Bson;
using Shared.MongoDB.Dto;
using Shared.MongoDB.Entity;
using Shared.MongoDB.Repository;

namespace SIGNAL.STORAGE.SERVICE;

public interface IServiceBase<T> where T : EntityBase
{
    IRepositoryBase<T> Repository { get; }
    Task<T?> GetAsync(ObjectId id);
    Task<List<T>> Search(Expression<Func<T, bool>> expression);
    Task<T?> SearchOne(Expression<Func<T, bool>> expression);
    Task<PagingObject<T>> Paging(Expression<Func<T, bool>> expression, int skip, int take);
    Task<PagingObject<T>> Paging<TKey>(Expression<Func<T, bool>> expression, int skip, int take, Expression<Func<T, TKey>>? sort = null);
    Task<List<T>> GetAllAsync();
    Task<int> Update(T entity);
    Task<int> UpdateRange(T[] entities);
    Task<int> Insert(T entity);
    Task<int> InsertRange(T[] entities);
    Task<int> DeleteRange(T[] entities);
    Task<int> Delete(ObjectId id);
    Task<long> Count(Expression<Func<T, bool>> expression);
}
