using MongoDB.Bson;
using MongoDB.Driver;
using Shared.MongoDB.Dto;
using Shared.MongoDB.Entity;
using System.Linq.Expressions;

namespace Shared.MongoDB.Repository;
public interface IRepositoryBase<T> where T : EntityBase
{
    IMongoCollection<T> MongoCollectionDB{ get; }
    Task<T?> GetAsync(ObjectId id);
    Task<List<T>> GetAllAsync();
    Task<List<T>> Search(Expression<Func<T, bool>> expression);
    Task<List<TResult>> SearchBySqlRaw<TResult>(string sqlText, params object[] objects);
    Task<TResult?> SearchOnceBySqlRaw<TResult>(string sqlText, params object[] objects);
    Task<PagingObject<T>> Paging(Expression<Func<T, bool>> expression, int skip, int take);
    Task<PagingObject<T>> Paging<TKey>(Expression<Func<T, bool>> expression, int skip, int take, Expression<Func<T, TKey>>? sort = null);
    Task<T?> SearchOne(Expression<Func<T, bool>> expression);
    Task<int> Update(T entity);
    Task<int> UpdateRange(T[] entities);
    Task<int> Insert(T entity);
    Task<int> InsertRange(T[] entities);
    Task<int> Delete(ObjectId id);
    Task<int> DeleteRange(T[] entities);
    Task<long> Count(Expression<Func<T, bool>> expression);
}
