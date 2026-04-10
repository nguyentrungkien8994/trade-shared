using Core.Database;
using Core.Database.Dto;
using Core.Database.Entity;
using Dapper;
using Shared.Database.Oracle.Builder;
using Shared.Database.Oracle.Factory;
using System.Linq.Expressions;
namespace Shared.Database.Oracle.Repository;

public class RepositoryBase<T, TId> : IRepositoryBaseOracle<T, TId> where T : IEntityBase<TId>
{
    private readonly IOracleConnectionFactory _factory;
    private readonly IOracleSqlBuilder _builderQuery;

    private readonly string _tableName;

    public RepositoryBase(IOracleConnectionFactory factory, IOracleSqlBuilder builderQuery)
    {
        _factory = factory;
        _tableName = EntityMetadata.GetTableName(typeof(T)); // hoặc custom mapping
        _builderQuery = builderQuery;
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
        var sql = _builderQuery.BuildGetAll<T>();
        var res = await conn.QueryAsync<T>(sql);
        return res == null ? new() : res.ToList();
    }

    public async Task<T> GetAsync(TId id)
    {
        using var conn = await _factory.CreateAsync();

        (string sql, object param) = _builderQuery.BuildGetById<T, TId>(id);
        var res = await conn.QueryFirstOrDefaultAsync<T>(sql, param);
        return res;
    }

    public async Task<PagingObject<T>> GetPaging(int skip, int take, IDictionary<string, object>? filters = null, IEnumerable<(string field, bool desc)>? sort = null)
    {
        using var conn = await _factory.CreateAsync();

        (string sql, object param) = _builderQuery.BuildPaging<T>(skip, take,filters,sort);
        var res = await conn.QueryAsync<T>(sql, param);
        return new PagingObject<T>() { Data = (res == null ? new() : res.ToList()), Skip = skip, Take = take };
    }

    public Task<int> InsertAsync(T entity)
    {
        throw new NotImplementedException();
    }

    public Task<int> InsertRangeAsync(T[] entities)
    {
        throw new NotImplementedException();
    }

    public async Task<IEnumerable<T>> Search(IDictionary<string, object>? filters = null)
    {
        using var conn = await _factory.CreateAsync();

        var type = typeof(T);
        var table = EntityMetadata.GetTableName(type);
        var props = EntityMetadata.GetProperties(type);

        var parameters = new Dictionary<string, object>();
        var ctx = new SqlParamContext();

        var where =_builderQuery.BuildWhere<T>(filters, props, parameters, ctx);

        var sql = $"SELECT * FROM {table} WHERE {where}";

        return await conn.QueryAsync<T>(sql, parameters);
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
