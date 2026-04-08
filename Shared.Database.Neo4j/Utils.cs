using Neo4j.Driver;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Shared.Database.Neo4j.Requests;
using Shared.Database.Neo4j.Responses;
using System.Security.Cryptography;
using System.Text;

namespace Shared.Database.Neo4j
{
    public class Utils
    {
        private int _paramIndex = 0;
        private readonly Dictionary<string, object> _params = new();

        public object ParserRecords(IEnumerable<IRecord> records)
        {
            var nodes = new Dictionary<string, object>();
            var edges = new List<object>();
            var edgeSet = new HashSet<string>();

            foreach (var record in records)
            {
                foreach (var key in record.Keys)
                {
                    var value = record[key];
                    Extract(value, nodes, edges, edgeSet);
                }
            }

            return new
            {
                nodes = nodes.Values.ToList(),
                relationships = edges
            };
        }

        // =========================
        // RECURSIVE EXTRACTOR
        // =========================
        private void Extract(object value,
            Dictionary<string, object> nodes,
            List<object> edges,
            HashSet<string> edgeSet)
        {
            if (value == null) return;

            // =========================
            // NODE
            // =========================
            if (value is INode node)
            {
                var id = node.ElementId.ToString();

                if (!nodes.ContainsKey(id))
                {
                    nodes[id] = new
                    {
                        id,
                        labels = node.Labels,
                        properties = node.Properties
                    };
                }

                return;
            }

            // =========================
            // RELATIONSHIP
            // =========================
            if (value is IRelationship rel)
            {
                var id = rel.ElementId.ToString();

                if (!edgeSet.Contains(id))
                {
                    edgeSet.Add(id);

                    edges.Add(new
                    {
                        id,
                        type = rel.Type,
                        from = rel.StartNodeElementId.ToString(),
                        to = rel.EndNodeElementId.ToString(),
                        properties = rel.Properties
                    });
                }

                return;
            }

            // =========================
            // PATH
            // =========================
            if (value is IPath path)
            {
                foreach (var n in path.Nodes)
                    Extract(n, nodes, edges, edgeSet);

                foreach (var r in path.Relationships)
                    Extract(r, nodes, edges, edgeSet);

                return;
            }

            // =========================
            // LIST / COLLECTION
            // =========================
            if (value is IEnumerable<object> list)
            {
                foreach (var item in list)
                {
                    Extract(item, nodes, edges, edgeSet);
                }

                return;
            }

            // =========================
            // MAP (dictionary)
            // =========================
            if (value is IDictionary<string, object> dict)
            {
                foreach (var v in dict.Values)
                {
                    Extract(v, nodes, edges, edgeSet);
                }

                return;
            }
        }

        public CypherQuery ParseRelationBetweenNodes(SearchParam dsl)
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
        public CypherQuery Parse(string json)
        {
            if(string.IsNullOrWhiteSpace(json))
                return new CypherQuery()
                {
                    Query = "CALL db.schema.visualization()",
                };
            var dsl = JsonConvert.DeserializeObject<SearchParam>(json);
            var _options = new GraphQueryOptions();
            var query = new StringBuilder();
            var whereParts = new List<string>();

            var hasRelations = dsl.Relations != null && dsl.Relations.Any();
            var isFindPath = dsl.Target != null;
            if (isFindPath)
                return ParseRelationBetweenNodes(dsl);

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

        //public CypherQuery Parse(string json)
        //{
        //    var dsl = JsonConvert.DeserializeObject<SearchParam>(json);

        //    if (dsl == null || string.IsNullOrEmpty(dsl.Node))
        //        throw new Exception("Invalid DSL");

        //    var alias = "n";

        //    var query = new StringBuilder();

        //    // MATCH node
        //    query.AppendLine($"MATCH ({alias}:{dsl.Node})");

        //    var returnItems = new List<string> { alias };
        //    var whereParts = new List<string>();

        //    // =========================
        //    // RELATIONS
        //    // =========================
        //    if (dsl.Relations != null && dsl.Relations.Any())
        //    {
        //        for (int i = 0; i < dsl.Relations.Count; i++)
        //        {
        //            var rel = dsl.Relations[i];

        //            var relVar = $"r{i}";
        //            var targetVar = $"m{i}";

        //            var depth = rel.Depth != null
        //                ? $"{rel.Depth.Min}..{rel.Depth.Max}"
        //                : "";

        //            var arrow = rel.Direction switch
        //            {
        //                "out" => $"-[{relVar}:{rel.Type}*{depth}]->",
        //                "in" => $"<-[{relVar}:{rel.Type}*{depth}]-",
        //                _ => $"-[{relVar}:{rel.Type}*{depth}]-"
        //            };

        //            query.AppendLine($"OPTIONAL MATCH ({alias}){arrow}({targetVar}:{rel.Target})");

        //            returnItems.Add(targetVar);

        //            // RELATION FILTER
        //            if (rel.Filter != null)
        //            {
        //                var relFilter = BuildFilter(
        //                    rel.Filter as JObject ?? JObject.FromObject(rel.Filter),
        //                    "rel"
        //                );

        //                whereParts.Add($"ALL(rel IN relationships({relVar}) WHERE {relFilter})");
        //            }
        //        }
        //    }

        //    // =========================
        //    // NODE FILTER
        //    // =========================
        //    if (dsl.Filter != null)
        //    {
        //        var filter = dsl.Filter as JObject ?? JObject.FromObject(dsl.Filter);
        //        var nodeWhere = BuildFilter(filter, alias);
        //        whereParts.Add(nodeWhere);
        //    }

        //    // =========================
        //    // WHERE
        //    // =========================
        //    if (whereParts.Any())
        //    {
        //        query.AppendLine("WHERE " + string.Join(" AND ", whereParts));
        //    }

        //    // =========================
        //    // RETURN
        //    // =========================
        //    query.AppendLine("RETURN " + string.Join(", ", returnItems));

        //    return new CypherQuery
        //    {
        //        Query = query.ToString(),
        //        Params = _params
        //    };
        //}

        //// =========================
        //// FILTER PARSER
        //// =========================
        //private string BuildFilter(JObject obj, string alias)
        //{
        //    var parts = new List<string>();

        //    foreach (var prop in obj.Properties())
        //    {
        //        if (prop.Name == "$and")
        //        {
        //            var sub = ((JArray)prop.Value)
        //                .Select(x => BuildFilter((JObject)x, alias));

        //            parts.Add($"({string.Join(" AND ", sub)})");
        //        }
        //        else if (prop.Name == "$or")
        //        {
        //            var sub = ((JArray)prop.Value)
        //                .Select(x => BuildFilter((JObject)x, alias));

        //            parts.Add($"({string.Join(" OR ", sub)})");
        //        }
        //        else if (prop.Name == "$not")
        //        {
        //            var sub = BuildFilter((JObject)prop.Value, alias);
        //            parts.Add($"NOT ({sub})");
        //        }
        //        else
        //        {
        //            parts.Add(BuildField(prop.Name, prop.Value, alias));
        //        }
        //    }

        //    return string.Join(" AND ", parts);
        //}

        //// =========================
        //// FIELD PARSER
        //// =========================
        //private string BuildField(string field, JToken token, string alias)
        //{
        //    if (token.Type == JTokenType.Object)
        //    {
        //        var obj = (JObject)token;
        //        var parts = new List<string>();

        //        foreach (var op in obj.Properties())
        //        {
        //            var param = NewParam(op.Value);

        //            parts.Add(op.Name switch
        //            {
        //                "$eq" => $"{alias}.{field} = ${param}",
        //                "$ne" => $"{alias}.{field} <> ${param}",
        //                "$gt" => $"{alias}.{field} > ${param}",
        //                "$gte" => $"{alias}.{field} >= ${param}",
        //                "$lt" => $"{alias}.{field} < ${param}",
        //                "$lte" => $"{alias}.{field} <= ${param}",
        //                "$in" => $"{alias}.{field} IN ${param}",
        //                "$nin" => $"NOT {alias}.{field} IN ${param}",
        //                "$contains" => $"{alias}.{field} CONTAINS ${param}",
        //                "$startsWith" => $"{alias}.{field} STARTS WITH ${param}",
        //                "$endsWith" => $"{alias}.{field} ENDS WITH ${param}",
        //                _ => throw new NotSupportedException(op.Name)
        //            });
        //        }

        //        return string.Join(" AND ", parts);
        //    }
        //    else
        //    {
        //        var param = NewParam(token);
        //        return $"{alias}.{field} = ${param}";
        //    }
        //}

        //// =========================
        //// PARAM HANDLER
        //// =========================
        //private string NewParam(JToken token)
        //{
        //    var name = $"p{_paramIndex++}";
        //    _params[name] = ConvertToken(token);
        //    return name;
        //}

        //private object ConvertToken(JToken token)
        //{
        //    return token.Type switch
        //    {
        //        JTokenType.Integer => token.Value<long>(),
        //        JTokenType.Float => token.Value<double>(),
        //        JTokenType.String => token.Value<string>(),
        //        JTokenType.Boolean => token.Value<bool>(),
        //        JTokenType.Array => token.Select(ConvertToken).ToList(),
        //        JTokenType.Null => null,
        //        _ => token.ToString()
        //    };
        //}



        //public CypherQuery Parse(string json)
        //{
        //    var dsl = JsonConvert.DeserializeObject<SearchParam>(json);

        //    if (dsl == null || string.IsNullOrEmpty(dsl.Node))
        //        throw new Exception("Invalid DSL");

        //    var alias = "n";

        //    var match = $"MATCH ({alias}:{dsl.Node})";

        //    string where = null;

        //    if (dsl.Filter != null)
        //    {
        //        var filterToken = dsl.Filter as JObject
        //                          ?? JObject.FromObject(dsl.Filter);

        //        where = BuildFilter(filterToken, alias);
        //    }

        //    var query = new StringBuilder();
        //    query.AppendLine(match);

        //    if (!string.IsNullOrEmpty(where))
        //        query.AppendLine($"WHERE {where}");

        //    query.AppendLine($"RETURN {alias}");

        //    return new CypherQuery
        //    {
        //        Query = query.ToString(),
        //        Params = _params
        //    };
        //}
        //public CypherQuery Parse(SearchParam dsl)
        //{
        //    if (dsl == null || string.IsNullOrEmpty(dsl.Node))
        //        throw new Exception("Invalid DSL");

        //    var alias = "n";

        //    var match = $"MATCH ({alias}:{dsl.Node})";

        //    string where = null;

        //    if (dsl.Filter != null)
        //    {
        //        var filterToken = dsl.Filter as JObject
        //                          ?? JObject.FromObject(dsl.Filter);

        //        where = BuildFilter(filterToken, alias);
        //    }

        //    var query = new StringBuilder();
        //    query.AppendLine(match);

        //    if (!string.IsNullOrEmpty(where))
        //        query.AppendLine($"WHERE {where}");

        //    query.AppendLine($"RETURN {alias}");

        //    return new CypherQuery
        //    {
        //        Query = query.ToString(),
        //        Params = _params
        //    };
        //}

        //// =========================
        //// FILTER PARSER
        //// =========================

        //private string BuildFilter(JObject obj, string alias)
        //{
        //    var parts = new List<string>();

        //    foreach (var prop in obj.Properties())
        //    {
        //        if (prop.Name == "$and")
        //        {
        //            var sub = ((JArray)prop.Value)
        //                .Select(x => BuildFilter((JObject)x, alias));

        //            parts.Add($"({string.Join(" AND ", sub)})");
        //        }
        //        else if (prop.Name == "$or")
        //        {
        //            var sub = ((JArray)prop.Value)
        //                .Select(x => BuildFilter((JObject)x, alias));

        //            parts.Add($"({string.Join(" OR ", sub)})");
        //        }
        //        else if (prop.Name == "$not")
        //        {
        //            var sub = BuildFilter((JObject)prop.Value, alias);
        //            parts.Add($"(NOT ({sub}))");
        //        }
        //        else
        //        {
        //            parts.Add(BuildField(prop.Name, prop.Value, alias));
        //        }
        //    }

        //    return string.Join(" AND ", parts);
        //}

        //// =========================
        //// FIELD PARSER
        //// =========================

        //private string BuildField(string field, JToken token, string alias)
        //{
        //    // Case: { "Age": { "$gt": 10 } }
        //    if (token.Type == JTokenType.Object)
        //    {
        //        var obj = (JObject)token;
        //        var parts = new List<string>();

        //        foreach (var op in obj.Properties())
        //        {
        //            var param = NewParam(op.Value);

        //            parts.Add(op.Name switch
        //            {
        //                "$eq" => $"{alias}.{field} = ${param}",
        //                "$ne" => $"{alias}.{field} <> ${param}",
        //                "$gt" => $"{alias}.{field} > ${param}",
        //                "$gte" => $"{alias}.{field} >= ${param}",
        //                "$lt" => $"{alias}.{field} < ${param}",
        //                "$lte" => $"{alias}.{field} <= ${param}",
        //                "$in" => $"{alias}.{field} IN ${param}",
        //                "$nin" => $"NOT {alias}.{field} IN ${param}",
        //                "$contains" => $"{alias}.{field} CONTAINS ${param}",
        //                "$startsWith" => $"{alias}.{field} STARTS WITH ${param}",
        //                "$endsWith" => $"{alias}.{field} ENDS WITH ${param}",
        //                _ => throw new NotSupportedException($"Operator {op.Name} not supported")
        //            });
        //        }

        //        return string.Join(" AND ", parts);
        //    }

        //    // Case: { "Status": "ACTIVE" }
        //    else
        //    {
        //        var param = NewParam(token);
        //        return $"{alias}.{field} = ${param}";
        //    }
        //}

        //// =========================
        //// PARAM HANDLER
        //// =========================

        //private string NewParam(JToken token)
        //{
        //    var name = $"p{_paramIndex++}";
        //    _params[name] = ConvertToken(token);
        //    return name;
        //}

        //private object ConvertToken(JToken token)
        //{
        //    return token.Type switch
        //    {
        //        JTokenType.Integer => token.Value<long>(),
        //        JTokenType.Float => token.Value<double>(),
        //        JTokenType.String => token.Value<string>(),
        //        JTokenType.Boolean => token.Value<bool>(),
        //        JTokenType.Array => token.Select(ConvertToken).ToList(),
        //        JTokenType.Null => null,
        //        _ => token.ToString()
        //    };
        //}
    }
}
