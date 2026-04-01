using Core.Database.Entity;
using System.Linq.Expressions;

namespace Core.Database;

public interface IServiceBase<T,TId,IRepo> where T : IEntityBase<TId> where IRepo : IRepositoryBase<T,TId>
{
    IRepo Repository { get; }
    Task<T> GetAsync(TId id);
    Task<List<T>> GetAllAsync();
    Task<T> SearchOneAsync(Expression<Func<T, bool>> expression);
    Task<List<T>> SearchAsync(Expression<Func<T, bool>> expression);
    Task<List<T>> SearchBySqlRawAsync(string sqlText, params object[] parameters);
    Task<T> SearchOnceBySqlRawAsync(string sqlText, params object[] parameters);
    Task<List<TResult>> SearchBySqlRawAsync<TResult>(string sqlText, params object[] parameters);
    Task<TResult?> SearchOnceBySqlRawAsync<TResult>(string sqlText, params object[] parameters);
    Task<int> UpdateAsync(T entity);
    Task<int> UpdateRangeAsync(T[] entities);
    Task<int> InsertAsync(T entity);
    Task<int> InsertRangeAsync(T[] entities);
    Task<int> DeleteAsync(TId id);
    Task<int> DeleteRangeAsync(T[] entities);
    Task<long> CountAsync(Expression<Func<T, bool>> expression);
}
public interface IServiceBase<T, TId>: IServiceBase<T,TId,IRepositoryBase<T,TId>> where T: IEntityBase<TId>
{

}
public interface IServiceBase<T>: IServiceBase<T,Guid> where T:IEntityBase<Guid>
{

}