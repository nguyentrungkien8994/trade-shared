using KLib.Core.Database;
using KLib.Core.Database.Entity;
using Neo4j.Driver;
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

    private static (string query, object parameters) BuildMerge(IEnumerable<object> upserts, string idKey = "id")
    {
        var list = upserts.ToList();
        if (!list.Any())
            throw new ArgumentException("Empty collection");

        var first = list.First();
        var label = typeof(T).GetTableName();
        var keyName = idKey;

        var rows = new List<Dictionary<string, object>>();
        var entityProps = typeof(T).GetProperties().Select(x => x.Name);
        foreach (var entity in list)
        {
            var dict = new Dictionary<string, object>();

            var props = entity.GetType().GetProperties();

            foreach (var prop in props)
            {
                var value = prop.GetValue(entity);
                if (entityProps.Contains(prop.Name))
                    dict[prop.Name] = value;
            }

            rows.Add(dict);
        }

        var cypher = $@"
UNWIND $rows AS row
MERGE (n:{label} {{{keyName}: row.{keyName}}})
SET n += row
";

        var parameters = new Dictionary<string, object>
        {
            ["rows"] = rows
        };

        return (cypher, parameters);
    }
    private static (string query, object parameters) BuildMergeRelationship(IEnumerable<Relationship> rels, string fromKey = "id", string toKey = "id")
    {
        var list = rels.ToList();
        if (!list.Any())
            throw new ArgumentException("Empty");

        var fromLabel = list.First().FromNode.Trim();
        var toLabel = list.First().ToNode.Trim();
        var relation = list.First().RelationName.Trim();
        var rows = list.Select(x => new Dictionary<string, object>
        {
            ["from"] = x.FromId,
            ["to"] = x.ToId
        }).ToList();
        var cypher = $@"
                UNWIND $rows AS row
                MERGE (a:{fromLabel} {{{fromKey}: row.from}})
                MERGE (b:{toLabel} {{{toKey}: row.to}})
                MERGE (a)-[r:{relation}]->(b)
                ";

        return (cypher, new Dictionary<string, object>
        {
            ["rows"] = rows
        });

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

    public virtual async Task<int> UpSertNodeAsync(IEnumerable<object> upserts,string idKey="id")
    {
        if (upserts == null || upserts.Count() == 0) return 0;
        (string cypher, object parameters) = BuildMerge(upserts, idKey);
        var summary = await _dataAccess.WriteScalarAsync(cypher, parameters);
        return summary.Counters.NodesCreated;
    }

    public virtual async Task<int> UpSertRelationshipAsync(IEnumerable<Relationship> rels, string fromKey = "id", string toKey = "id")
    {
        if (rels == null || rels.Count() == 0) return 0;
        (string cypher, object parameters) = BuildMergeRelationship(rels, fromKey, toKey);
        var summary = await _dataAccess.WriteScalarAsync(cypher, parameters);
        return summary.Counters.RelationshipsCreated;
    }
}
