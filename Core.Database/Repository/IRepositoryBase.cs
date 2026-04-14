
using KLib.Core.Database.Dto;
using KLib.Core.Database.Entity;

namespace KLib.Core.Database;

public interface IRepositoryBase
{
    Task<IEnumerable<IDictionary<string, object>>> GetAllObjectAsync(string objName);
    Task<IEnumerable<IDictionary<string, object>>> SearchObjectAsync(string objName,IDictionary<string, object>? filters = null, IEnumerable<(string field, bool desc)>? sort = null);
    Task<PagingObject<IDictionary<string, object>>> PagingObjectAsync(string objName,int skip, int take, IDictionary<string, object>? filters = null, IEnumerable<(string field, bool desc)>? sort = null);
}
public interface IRepositoryBase<T, TId> where T : IEntityKey<TId>
{
    Task<T?> GetAsync(TId id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<T?> SearchOneAsync(IDictionary<string, object>? filters = null, IEnumerable<(string field, bool desc)>? sort = null);
    Task<IEnumerable<T>> SearchAsync(IDictionary<string, object>? filters = null, IEnumerable<(string field, bool desc)>? sort = null);
    Task<PagingObject<T>> PagingAsync(int skip, int take, IDictionary<string, object>? filters = null, IEnumerable<(string field, bool desc)>? sort = null);
    //Task<IEnumerable<T>> SearchBySqlRawAsync(string sqlText, params object[] parameters);
    //Task<T?> SearchOnceBySqlRawAsync(string sqlText, params object[] parameters);
    //Task<List<TResult>> SearchBySqlRawAsync<TResult>(string sqlText, params object[] parameters);
    //Task<TResult?> SearchOnceBySqlRawAsync<TResult>(string sqlText, params object[] parameters);
    //Task<int> UpdateRangeAsync(T[] entities);
    //Task<int> DeleteRangeAsync(T[] entities);
    //Task<long> CountAsync(Expression<Func<T, bool>> expression);
    Task<int> InsertAsync(T entity);
    Task<int> UpdateAsync(T entity);
    Task<int> DeleteAsync(TId id);
    Task<int> InsertBulkAsync(T[] entities);
}
