using Dapper;
using KLib.Core.Database;
using KLib.Core.Database.Dto;
using KLib.Core.Database.Entity;
using Newtonsoft.Json;
using Shared.Database.Oracle.Builder;
using Shared.Database.Oracle.Factory;
namespace Shared.Database.Oracle.Repository;

public class RepositoryBaseOracle : IRepositoryBaseOracle
{
    private readonly IOracleConnectionFactory _factory;
    private readonly IOracleSqlBuilder _builderQuery;

    public RepositoryBaseOracle(IOracleConnectionFactory factory, IOracleSqlBuilder builderQuery)
    {
        _factory = factory;
        _builderQuery = builderQuery;
    }

    public virtual async Task<IEnumerable<IDictionary<string, object>>> GetAllObjectAsync(string tableName)
    {
        using var conn = await _factory.CreateAsync();
        var sql = _builderQuery.BuildGetAll(tableName);
        var res = await conn.QueryAsync(sql);
        string json = JsonConvert.SerializeObject(res);
        var parserResult = JsonConvert.DeserializeObject<IEnumerable<IDictionary<string, object>>>(json);
        return parserResult == null ? Enumerable.Empty<IDictionary<string, object>>() : parserResult;
    }

    public virtual async Task<PagingObject<IDictionary<string, object>>> PagingObjectAsync(string tableName, int skip, int take, IDictionary<string, object>? filters = null, IEnumerable<(string field, bool desc)>? sort = null)
    {
        using var conn = await _factory.CreateAsync();
        (string sql, object param) = _builderQuery.BuildPaging(tableName, skip, take, filters, sort);
        var res = await conn.QueryAsync(sql,param);
        string json = JsonConvert.SerializeObject(res);
        var parserResult = JsonConvert.DeserializeObject<IEnumerable<IDictionary<string, object>>>(json);
        var data = (parserResult == null) ? Enumerable.Empty<IDictionary<string, object>>() : parserResult;
        return new PagingObject<IDictionary<string, object>> { Data = data, Skip=skip, Take= take, TotalCount = 0 };
    }

    public virtual async Task<IEnumerable<IDictionary<string, object>>> SearchObjectAsync(string tableName, IDictionary<string, object>? filters = null, IEnumerable<(string field, bool desc)>? sort = null)
    {
        using var conn = await _factory.CreateAsync();
        (string sql, object param) = _builderQuery.BuildPaging(tableName, 0, 500, filters, sort);
        var res = await conn.QueryAsync(sql, param);
        string json = JsonConvert.SerializeObject(res);
        var parserResult = JsonConvert.DeserializeObject<IEnumerable<IDictionary<string, object>>>(json);
        var data = (parserResult == null) ? Enumerable.Empty<IDictionary<string, object>>() : parserResult;
        return data;
    }

}
public class RepositoryBaseOracle<T, TId> : IRepositoryBaseOracle<T, TId> where T : IEntityKey<TId>
{
    private readonly IOracleConnectionFactory _factory;
    private readonly IOracleSqlBuilder _builderQuery;

    private readonly string _tableName;

    public RepositoryBaseOracle(IOracleConnectionFactory factory, IOracleSqlBuilder builderQuery)
    {
        _factory = factory;
        _tableName = EntityMetadata.GetTableName(typeof(T)); // hoặc custom mapping
        _builderQuery = builderQuery;
    }

    public virtual Task<int> DeleteAsync(TId id)
    {
        throw new NotImplementedException();
    }

    public virtual async Task<IEnumerable<T>> GetAllAsync()
    {
        using var conn = await _factory.CreateAsync();
        var sql = _builderQuery.BuildGetAll(typeof(T).GetTableName());
        var res = await conn.QueryAsync<T>(sql);
        return res == null ? new() : res.ToList();
    }

    public virtual async Task<T?> GetAsync(TId id)
    {
        using var conn = await _factory.CreateAsync();

        (string sql, object param) = _builderQuery.BuildGetById<T, TId>(id);
        var res = await conn.QueryFirstOrDefaultAsync<T>(sql, param);
        return res;
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

    public virtual async Task<IEnumerable<T>> SearchAsync(IDictionary<string, object>? filters = null, IEnumerable<(string field, bool desc)>? sort = null)
    {
        using var conn = await _factory.CreateAsync();

        var type = typeof(T);
        var table = EntityMetadata.GetTableName(type);
        var props = EntityMetadata.GetProperties(type);

        var parameters = new Dictionary<string, object>();
        var ctx = new SqlParamContext();

        var where = _builderQuery.BuildWhere<T>(filters, props, parameters, ctx);

        var sql = $"SELECT * FROM {table} WHERE {where}";

        return await conn.QueryAsync<T>(sql, parameters);
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