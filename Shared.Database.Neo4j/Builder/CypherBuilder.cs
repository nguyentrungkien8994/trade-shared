using KLib.Core.Database;
using Shared.Database.Neo4j.Requests;
using System.Reflection;
using System.Text;

namespace Shared.Database.Neo4j.Builder;

public class CypherBuilder : ICypherBuilder
{
    #region "Private"
    private static void TrimComma(StringBuilder sb)
    {
        if (sb.Length >= 2 && sb[^2] == ',')
        {
            sb.Length -= 2;
        }
    }
    private static PropertyInfo[] GetProperties(Type t)
        => t.GetProperties(BindingFlags.Public | BindingFlags.Instance);
    #endregion

    #region "Public"

    public (string query, object parameters) BuildCreate<T>(T entity)
    {
        var props = GetProperties(typeof(T));

        var sb = new StringBuilder();
        var parameters = new Dictionary<string, object?>();

        sb.Append($"CREATE (n:{typeof(T).GetTableName()} {{ ");

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

    public string BuildGetAll(string entityName)
    {
        string cypher = $"MATCH(n:{entityName}) return n";
        return cypher;
    }

    public (string query, object parameters) BuildMergeNode<T>(IEnumerable<object> nodes, string idKey = "id")
    {
        if (nodes.Count() == 0)
            throw new ArgumentException("Empty collection");

        var label = typeof(T).GetTableName();
        var keyName = idKey;

        var rows = new List<Dictionary<string, object>>();
        var entityProps = typeof(T).GetProperties().Select(x => x.Name);
        foreach (var entity in nodes)
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

    public (string query, object parameters) BuildMergeRelationship(IEnumerable<Relationship> rels, string fromKey = "id", string toKey = "id")
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

    public (string query, object parameters) BuildUpdate<T>(T entity)
    {
        var props = GetProperties(typeof(T));

        var sb = new StringBuilder();
        var parameters = new Dictionary<string, object?>();

        var idProp = props.First(p => p.Name == "id");
        var idValue = idProp.GetValue(entity);

        sb.Append($"MATCH (n:{typeof(T).GetTableName()} {{id: $id}}) SET ");

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

    public (string query, object parameters) BuildDelete<T>(object id)
    {
        string keyName = typeof(T).GetIdName() ?? "id";
        var parameters = new Dictionary<string, object?>()
        {
            { keyName,id }
        };
        string cypher = $"MATCH(n:{typeof(T).Name}{{{keyName}:\"{id}\"}}) Delete n";
        return (cypher, parameters);
    }

    public (string query, object parameters) BuildGet<T>(object id)
    {
        string keyName = typeof(T).GetIdName() ?? "id";
        var parameters = new Dictionary<string, object?>()
        {
            { keyName,id }
        };
        string cypher = $"MATCH(n:{typeof(T).Name}{{{keyName}:\"{id}\"}}) return n";
        return (cypher, parameters);
    }

    public (string query, object parameters) BuildMergeNode(IEnumerable<IDictionary<string,object>> nodes, string nodeName, string idKey = "id")
    {
        if (nodes.Count() == 0)
            throw new ArgumentException("Empty collection");

        var label = nodeName;
        var keyName = idKey;

        var cypher = $@"
                    UNWIND $rows AS row
                    MERGE (n:{label} {{{keyName}: row.{keyName}}})
                    SET n += row
                    ";

        var parameters = new Dictionary<string, object>
        {
            ["rows"] = nodes
        };

        return (cypher, parameters);
    }

    #endregion
}
