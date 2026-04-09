using Core.Database;
using Core.Database.Entity;
using Dapper;
using Shared.Database.Oracle.Factory;
using System.Linq.Expressions;
using System.Reflection;

namespace Shared.Database.Oracle.Repository;

public class RepositoryBase<T, TId> : IRepositoryBase<T, TId> where T : IEntityBase<TId>
{
    private readonly IOracleConnectionFactory _factory;

    private readonly string _tableName;

    public RepositoryBase(IOracleConnectionFactory factory)
    {
        _factory = factory;
        _tableName = typeof(T).Name.ToUpper(); // hoặc custom mapping
    }
    public Task<long> CountAsync(Expression<Func<T, bool>> expression)
    {
        throw new NotImplementedException();
    }

    public Task<int> DeleteAsync(TId id)
    {
        throw new NotImplementedException();
    }

    public Task<int> DeleteRangeAsync(T[] entities)
    {
        throw new NotImplementedException();
    }

    public async Task<List<T>> GetAllAsync()
    {
        using var conn = await _factory.CreateAsync();
        var sql = $"SELECT * FROM {_tableName}";
        var res = await conn.QueryAsync<T>(sql);
        return res == null ? new() : res.ToList();
    }

    public async Task<T> GetAsync(TId id)
    {
        using var conn = await _factory.CreateAsync();

        var sql = $"SELECT * FROM {_tableName} WHERE ID = :id";
        var res = await conn.QueryFirstOrDefaultAsync<T>(sql, new { id });
        return res;
    }

    public Task<int> InsertAsync(T entity)
    {
        throw new NotImplementedException();
    }

    public Task<int> InsertRangeAsync(T[] entities)
    {
        throw new NotImplementedException();
    }

    public Task<List<T>> SearchAsync(Expression<Func<T, bool>> expression)
    {
        throw new NotImplementedException();
    }

    public Task<List<T>> SearchBySqlRawAsync(string sqlText, params object[] parameters)
    {
        throw new NotImplementedException();
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
        throw new NotImplementedException();
    }

    public Task<int> UpdateAsync(T entity)
    {
        throw new NotImplementedException();
    }

    public Task<int> UpdateRangeAsync(T[] entities)
    {
        throw new NotImplementedException();
    }
}
