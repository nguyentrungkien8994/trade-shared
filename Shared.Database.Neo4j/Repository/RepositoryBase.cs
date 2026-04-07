using Core.Database.Entity;
using Shared.Database.Neo4j.DataAccess;
using Shared.Database.Neo4j.Requests;
using Shared.Database.Neo4j.Responses;
using System.Reflection;
using System.Text;

namespace Shared.Database.Neo4j.Repository;

public class RepositoryBase<T, TId> : IRepositoryBaseNeo4j<T, TId> where T : IEntityBase<TId>
{
    private readonly IDataAccess _dataAccess;
    public RepositoryBase(IDataAccess dataAccess)
    {
        _dataAccess = dataAccess;
    }
    private static PropertyInfo[] GetProperties()
        => typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
    private static void TrimComma(StringBuilder sb)
    {
        if (sb.Length >= 2 && sb[^2] == ',')
        {
            sb.Length -= 2;
        }
    }
    private static (string query, object parameters) BuildCreate(T entity)
    {
        var props = GetProperties();

        var sb = new StringBuilder();
        var parameters = new Dictionary<string, object?>();

        sb.Append($"CREATE (n:{typeof(T).Name} {{ ");

        foreach (var prop in props)
        {
            var name = prop.Name;
            sb.Append($"{name}: ${name}, ");

            parameters[name] = prop.GetValue(entity);
        }
        TrimComma(sb);
        sb.Append(" }) RETURN n");

        return (sb.ToString(), parameters);
    }
    private static (string query, object parameters) BuildUpdate(T entity)
    {
        var props = GetProperties();

        var sb = new StringBuilder();
        var parameters = new Dictionary<string, object?>();

        var idProp = props.First(p => p.Name == "id");
        var idValue = idProp.GetValue(entity);

        sb.Append($"MATCH (n:{typeof(T).Name} {{id: $id}}) SET ");

        parameters["id"] = idValue;

        foreach (var prop in props)
        {
            if (prop.Name == "id") continue;

            sb.Append($"n.{prop.Name} = ${prop.Name}, ");
            parameters[prop.Name] = prop.GetValue(entity);
        }
        TrimComma(sb);
        sb.Append(" RETURN n");

        return (sb.ToString(), parameters);
    }
    public virtual async Task<int> DeleteAsync(TId id)
    {
        string cypher = $"MATCH(n:{typeof(T).Name}{{id:\"{id}\"}}) Delete n";
        var summary = await _dataAccess.WriteScalarAsync(cypher, new { id = id });
        return summary.Counters.NodesDeleted;
    }

    public virtual async Task<IEnumerable<T>> GetAllAsync()
    {
        string cypher = $"MATCH(n:{typeof(T).Name}) return n";
        return await _dataAccess.ReadAsync<T>(cypher);
    }

    public virtual async Task<T?> GetAsync(TId id)
    {
        string cypher = $"MATCH(n:{typeof(T).Name}{{id:{id}}}) return n";
        var records = await _dataAccess.ReadAsync<T>(cypher, new { id = id });
        return records.FirstOrDefault();
    }

    public virtual async Task<T?> InsertAsync(T entity)
    {
        (string cypher, object parameters) = BuildCreate(entity);
        return await _dataAccess.WriteAsync<T>(cypher, parameters);
    }

    public virtual async Task<T?> UpdateAsync(T entity)
    {
        (string cypher, object parameters) = BuildUpdate(entity);
        return await _dataAccess.WriteAsync<T>(cypher, parameters);
    }

    public virtual async Task<object?> SearchNode(CypherQuery cypher)
    {
        if (cypher == null) return null;
        var records = await _dataAccess.WriteAsync(cypher.Query, cypher.Params);
        Utils utils = new();
        var results = utils.ParserRecords(records);
        return results;
    }
}
