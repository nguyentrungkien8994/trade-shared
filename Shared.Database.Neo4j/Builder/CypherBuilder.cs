using KLib.Core.Database;
using Neo4j.Driver;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Shared.Database.Neo4j.Requests;
using Shared.Database.Neo4j.Responses;
using System.Reflection;
using System.Text;

namespace Shared.Database.Neo4j.Builder;

public class CypherBuilder : ICypherBuilder
{
    #region "Private"
    private int _paramIndex = 0;
    private readonly Dictionary<string, object> _params = new();

    

    private string BuildDepth(RelationDsl rel, GraphQueryOptions options)
    {
        int min, max;

        if (rel.Depth == null)
        {
            // AUTO INJECT
            min = options.DefaultMinDepth;
            max = options.DefaultMaxDepth;
        }
        else
        {
            min = rel.Depth.Min <= 0 ? 1 : rel.Depth.Min;
            max = rel.Depth.Max <= 0 ? options.DefaultMaxDepth : rel.Depth.Max;

            // GUARD: không cho vượt quá giới hạn hệ thống
            if (max > options.HardMaxDepth)
                max = options.HardMaxDepth;

            if (min > max)
                min = max;
        }

        return $"*{min}..{max}";
    }
    // =========================
    // FILTER PARSER
    // =========================
    private string BuildFilter(JObject obj, string alias)
    {
        var parts = new List<string>();

        foreach (var prop in obj.Properties())
        {
            if (prop.Name == "$and")
            {
                var sub = ((JArray)prop.Value)
                    .Select(x => BuildFilter((JObject)x, alias));

                parts.Add($"({string.Join(" AND ", sub)})");
            }
            else if (prop.Name == "$or")
            {
                var sub = ((JArray)prop.Value)
                    .Select(x => BuildFilter((JObject)x, alias));

                parts.Add($"({string.Join(" OR ", sub)})");
            }
            else
            {
                parts.Add(BuildField(prop.Name, prop.Value, alias));
            }
        }

        return string.Join(" AND ", parts);
    }

    // =========================
    // FIELD PARSER
    // =========================
    private string BuildField(string field, JToken token, string alias)
    {
        if (token.Type == JTokenType.Object)
        {
            var obj = (JObject)token;
            var parts = new List<string>();

            foreach (var op in obj.Properties())
            {
                var param = NewParam(op.Value);

                parts.Add(op.Name switch
                {
                    "$eq" => $"{alias}.{field} = ${param}",
                    "$neq" => $"{alias}.{field} <> ${param}",
                    "$gt" => $"{alias}.{field} > ${param}",
                    "$gte" => $"{alias}.{field} >= ${param}",
                    "$lt" => $"{alias}.{field} < ${param}",
                    "$lte" => $"{alias}.{field} <= ${param}",
                    "$in" => $"{alias}.{field} IN ${param}",
                    _ => throw new NotSupportedException(op.Name)
                });
            }

            return string.Join(" AND ", parts);
        }
        else
        {
            var param = NewParam(token);
            return $"{alias}.{field} = ${param}";
        }
    }

    // =========================
    // PARAM
    // =========================
    private string NewParam(JToken token)
    {
        var name = $"p{_paramIndex++}";
        _params[name] = ConvertToken(token);
        return name;
    }

    private object ConvertToken(JToken token)
    {
        return token.Type switch
        {
            JTokenType.Integer => token.Value<long>(),
            JTokenType.Float => token.Value<double>(),
            JTokenType.String => token.Value<string>(),
            JTokenType.Boolean => token.Value<bool>(),
            JTokenType.Array => token.Select(ConvertToken).ToList(),
            _ => token.ToString()
        };
    }

    private CypherQuery BuildRelationBetweenNodes(SearchParam dsl)
    {
        var query = new StringBuilder();
        var whereParts = new List<string>();

        // =========================
        // START NODE
        // =========================
        query.AppendLine($"MATCH (n:{dsl.Node})");

        if (dsl.Filter != null)
        {
            var f = dsl.Filter as JObject ?? JObject.FromObject(dsl.Filter);
            whereParts.Add(BuildFilter(f, "n"));
        }

        // =========================
        // TARGET NODE
        // =========================
        query.AppendLine($"MATCH (t:{dsl.Target.Node})");

        if (dsl.Target.Filter != null)
        {
            var f = dsl.Target.Filter as JObject ?? JObject.FromObject(dsl.Target.Filter);
            whereParts.Add(BuildFilter(f, "t"));
        }

        if (whereParts.Any())
        {
            query.AppendLine("WHERE " + string.Join(" AND ", whereParts));
        }

        // =========================
        // PATH
        // =========================
        var maxDepth = 10;

        query.AppendLine($"MATCH p = shortestPath((n)-[r*1..{maxDepth}]-(t))");

        // =========================
        // REL FILTER
        // =========================
        if (dsl.Relations != null && dsl.Relations.Any())
        {
            var types = dsl.Relations
                .Where(x => !string.IsNullOrEmpty(x.Type))
                .Select(x => $"'{x.Type}'");

            if (types.Any())
            {
                query.AppendLine(
                    $"WHERE ALL(rel IN r WHERE type(rel) IN [{string.Join(",", types)}])"
                );
            }
        }

        query.AppendLine("RETURN p");

        return new CypherQuery
        {
            Query = query.ToString(),
            Params = _params
        };
    }
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

    public (string query, object parameters) BuildMergeNode(IEnumerable<IDictionary<string, object>> nodes, string nodeName, string idKey = "id")
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

    public CypherQuery BuildDynamicCypher(string json)
    {
        SearchParam? searchParam=null;
        if (!string.IsNullOrWhiteSpace(json))
        {
            searchParam = JsonConvert.DeserializeObject<SearchParam>(json);
        }
        return BuildDynamicCypher(searchParam);
    }

    public CypherQuery BuildDynamicCypher(SearchParam? searchParam)
    {
        if (searchParam == null)
            return new CypherQuery()
            {
                Query = "CALL db.schema.visualization()",
            };
        var dsl = searchParam;
        var _options = new GraphQueryOptions();
        var query = new StringBuilder();
        var whereParts = new List<string>();

        var hasRelations = dsl.Relations != null && dsl.Relations.Any();
        var isFindPath = dsl.Target != null;
        if (isFindPath)
            return BuildRelationBetweenNodes(dsl);

        // =========================
        // MATCH NODE (anchor index)
        // =========================
        if (string.IsNullOrWhiteSpace(dsl.Node))
        {
            query.AppendLine($"MATCH (n)");
        }
        else
        {
            query.AppendLine($"MATCH (n:{dsl.Node})");
        }

        if (dsl.Filter != null)
        {
            var filter = dsl.Filter as JObject ?? JObject.FromObject(dsl.Filter);
            whereParts.Add(BuildFilter(filter, "n"));
        }

        if (whereParts.Any())
        {
            query.AppendLine("WHERE " + string.Join(" AND ", whereParts));
        }

        // =========================
        // CASE 1: NO RELATION
        // =========================
        if (!hasRelations)
        {
            query.AppendLine(@"
                RETURN 
                  collect(DISTINCT n) AS nodes,
                  [] AS relationships
                ");

            return new CypherQuery
            {
                Query = query.ToString(),
                Params = _params
            };
        }

        // =========================
        // CASE 2: HAS RELATION
        // =========================

        var nodeCollects = new List<string>();
        var relCollects = new List<string>();

        for (int i = 0; i < dsl.Relations.Count; i++)
        {
            var rel = dsl.Relations[i];

            var relVar = $"r{i}";
            var nodeVar = $"m{i}";
            var pathVar = $"p{i}";

            // =========================
            // TYPE
            // =========================
            var type = string.IsNullOrEmpty(rel.Type)
                ? ""
                : $":{rel.Type}";

            // =========================
            // DEPTH
            // =========================
            //var depth = rel.Depth != null
            //    ? $"*{rel.Depth.Min}..{rel.Depth.Max}"
            //    : "";
            var depth = BuildDepth(rel, _options);

            // =========================
            // RELATION BODY
            // =========================
            var relBody = $"[{relVar}{type}{depth}]";

            // =========================
            // TARGET NODE
            // =========================
            var target = string.IsNullOrEmpty(rel.Target)
                ? "()"
                : $"({nodeVar}:{rel.Target})";

            // =========================
            // DIRECTION
            // =========================
            var pattern = rel.Direction switch
            {
                "out" => $"-{relBody}->{target}",
                "in" => $"<-{relBody}-{target}",
                _ => $"-{relBody}-{target}" // default BOTH
            };

            // =========================
            // MATCH PATH
            // =========================
            query.AppendLine(
                $"OPTIONAL MATCH {pathVar} = (n){pattern}"
            );

            // =========================
            // RELATION FILTER
            // =========================
            if (rel.Filter != null)
            {
                var relFilter = BuildFilter(
                    rel.Filter as JObject ?? JObject.FromObject(rel.Filter),
                    "rel"
                );

                query.AppendLine(
                    $"AND ALL(rel IN relationships({pathVar}) WHERE {relFilter})"
                );
            }

            // =========================
            // COLLECT
            // =========================
            nodeCollects.Add($"collect(DISTINCT nodes({pathVar}))");
            relCollects.Add($"collect(DISTINCT relationships({pathVar}))");
        }

        query.AppendLine($@"
            RETURN 
                {string.Join(" + ", nodeCollects)} AS nodeGroups,
                {string.Join(" + ", relCollects)} AS relGroups
            ");

        return new CypherQuery
        {
            Query = query.ToString(),
            Params = _params
        };
    }

    #endregion
}
