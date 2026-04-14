using KLib.Core.Database.Dto;
using KLib.Core.Database.Entity;
using System.Linq.Expressions;

namespace KLib.Core.Database;

public interface IServiceBase
{
    Task<IEnumerable<IDictionary<string, object>>> GetAllObjectAsync(string objName);
    Task<IEnumerable<IDictionary<string, object>>> SearchObjectAsync(string objName, IDictionary<string, object>? filters = null, IEnumerable<(string field, bool desc)>? sort = null);
    Task<PagingObject<IDictionary<string, object>>> PagingObjectAsync(string objName, int skip, int take, IDictionary<string, object>? filters = null, IEnumerable<(string field, bool desc)>? sort = null);
}
public interface IServiceBase<T, TId, IRepo> where T : IEntityKey<TId> where IRepo : IRepositoryBase<T, TId>
{
    IRepo Repository { get; }
    Task<T?> GetAsync(TId id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<T?> SearchOneAsync(IDictionary<string, object>? filters = null, IEnumerable<(string field, bool desc)>? sort = null);
    Task<IEnumerable<T>> SearchAsync(IDictionary<string, object>? filters = null, IEnumerable<(string field, bool desc)>? sort = null);
    Task<PagingObject<T>> PagingAsync(int skip, int take, IDictionary<string, object>? filters = null, IEnumerable<(string field, bool desc)>? sort = null);
    Task<int> InsertAsync(T entity);
    Task<int> UpdateAsync(T entity);
    Task<int> DeleteAsync(TId id);
    Task<int> InsertBulkAsync(T[] entities);

    //Task<T> GetAsync(TId id);
    //Task<List<T>> GetAllAsync();
    //Task<T> SearchOneAsync(Expression<Func<T, bool>> expression);
    //Task<List<T>> SearchAsync(Expression<Func<T, bool>> expression);
    //Task<List<T>> SearchBySqlRawAsync(string sqlText, params object[] parameters);
    //Task<T> SearchOnceBySqlRawAsync(string sqlText, params object[] parameters);
    //Task<List<TResult>> SearchBySqlRawAsync<TResult>(string sqlText, params object[] parameters);
    //Task<TResult?> SearchOnceBySqlRawAsync<TResult>(string sqlText, params object[] parameters);
    //Task<int> UpdateAsync(T entity);
    //Task<int> UpdateRangeAsync(T[] entities);
    //Task<int> InsertAsync(T entity);
    //Task<int> InsertRangeAsync(T[] entities);
    //Task<int> DeleteAsync(TId id);
    //Task<int> DeleteRangeAsync(T[] entities);
    //Task<long> CountAsync(Expression<Func<T, bool>> expression);
}
public interface IServiceBase<T, TId> : IServiceBase<T, TId, IRepositoryBase<T, TId>> where T : IEntityKey<TId>
{

}
public interface IServiceBase<T> : IServiceBase<T, Guid> where T : IEntityKey<Guid>
{

}