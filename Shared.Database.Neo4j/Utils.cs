using Neo4j.Driver;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Shared.Database.Neo4j.Requests;
using Shared.Database.Neo4j.Responses;
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

                    // ===== NODE =====
                    if (value is INode node)
                    {
                        //var id = GetNodeId(node);
                        var id = node.ElementId;

                        if (!nodes.ContainsKey(id))
                        {
                            nodes[id] = new
                            {
                                id,
                                labels = node.Labels,
                                properties = node.Properties
                            };
                        }
                    }
                    // ===== List object =====
                    if (value is List<object> nodearr)
                    {
                        foreach (var item in nodearr)
                        {
                            if (item is INode node2)
                            {
                                var id = node2.ElementId;

                                if (!nodes.ContainsKey(id))
                                {
                                    nodes[id] = new
                                    {
                                        id,
                                        labels = node2.Labels,
                                        properties = node2.Properties
                                    };
                                }
                            }
                            else if (item is IRelationship rel2)
                            {
                                var source = rel2.StartNodeElementId;
                                var target = rel2.EndNodeElementId;

                                var keyEdge = $"{source}-{target}-{rel2.Type}";
                                if (edgeSet.Contains(keyEdge)) continue;

                                edgeSet.Add(keyEdge);

                                edges.Add(new
                                {
                                    id = keyEdge,
                                    source,
                                    target,
                                    type = rel2.Type,
                                    properties = rel2.Properties
                                });
                            }
                        }

                    }
                    // ===== RELATION =====
                    else if (value is IRelationship rel)
                    {
                        var source = rel.StartNodeElementId;
                        var target = rel.EndNodeElementId;

                        var keyEdge = $"{source}-{target}-{rel.Type}";
                        if (edgeSet.Contains(keyEdge)) continue;

                        edgeSet.Add(keyEdge);

                        edges.Add(new
                        {
                            id = keyEdge,
                            source,
                            target,
                            type = rel.Type,
                            properties = rel.Properties
                        });
                    }

                    else if (value is IPath path)
                    {
                        // ===== NODES =====
                        foreach (var objnode in path.Nodes)
                        {
                            //var id = GetNodeId(objnode);
                            var id = objnode.ElementId;

                            if (!nodes.ContainsKey(id))
                            {
                                nodes[id] = new
                                {
                                    id,
                                    labels = objnode.Labels,
                                    properties = objnode.Properties
                                };
                            }
                        }

                        // ===== RELATIONSHIPS =====
                        foreach (var objrel in path.Relationships)
                        {
                            var source = objrel.StartNodeElementId;
                            var target = objrel.EndNodeElementId;

                            var strkey = $"{source}-{target}-{objrel.Type}";
                            if (edgeSet.Contains(strkey)) continue;

                            edgeSet.Add(strkey);

                            edges.Add(new
                            {
                                id = strkey,
                                source,
                                target,
                                type = objrel.Type,
                                properties = objrel.Properties
                            });
                        }
                    }


                    //// ===== PATH ===== (QUAN TRỌNG)
                    //else if (value is IPath path)
                    //{
                    //    foreach (var objnode in path.Nodes)
                    //    {
                    //        var id = GetNodeId(objnode);

                    //        if (!nodes.ContainsKey(id))
                    //        {
                    //            nodes[id] = new
                    //            {
                    //                id,
                    //                labels = objnode.Labels,
                    //                properties = objnode.Properties
                    //            };
                    //        }
                    //    }

                    //    foreach (var objrel in path.Relationships)
                    //    {
                    //        var source = objrel.StartNodeElementId;
                    //        var target = objrel.EndNodeElementId;

                    //        var keyEdge = $"{source}-{target}-{objrel.Type}";
                    //        if (edgeSet.Contains(keyEdge)) continue;

                    //        edgeSet.Add(keyEdge);

                    //        edges.Add(new
                    //        {
                    //            id = keyEdge,
                    //            source,
                    //            target,
                    //            type = objrel.Type,
                    //            properties = objrel.Properties
                    //        });
                    //    }
                    //}
                }
            }
            return new { nodes = nodes.Values, edges = edges, edgeSet = edgeSet };
        }

        public CypherQuery Parse(string json)
        {
            var dsl = JsonConvert.DeserializeObject<SearchParam>(json);

            if (dsl == null || string.IsNullOrEmpty(dsl.Node))
                throw new Exception("Invalid DSL");

            var alias = "n";

            var query = new StringBuilder();

            // MATCH node
            query.AppendLine($"MATCH ({alias}:{dsl.Node})");

            var returnItems = new List<string> { alias };
            var whereParts = new List<string>();

            // =========================
            // RELATIONS
            // =========================
            if (dsl.Relations != null && dsl.Relations.Any())
            {
                for (int i = 0; i < dsl.Relations.Count; i++)
                {
                    var rel = dsl.Relations[i];

                    var relVar = $"r{i}";
                    var targetVar = $"m{i}";

                    var depth = rel.Depth != null
                        ? $"{rel.Depth.Min}..{rel.Depth.Max}"
                        : "";

                    var arrow = rel.Direction switch
                    {
                        "out" => $"-[{relVar}:{rel.Type}*{depth}]->",
                        "in" => $"<-[{relVar}:{rel.Type}*{depth}]-",
                        _ => $"-[{relVar}:{rel.Type}*{depth}]-"
                    };

                    query.AppendLine($"OPTIONAL MATCH ({alias}){arrow}({targetVar}:{rel.Target})");

                    returnItems.Add(targetVar);

                    // RELATION FILTER
                    if (rel.Filter != null)
                    {
                        var relFilter = BuildFilter(
                            rel.Filter as JObject ?? JObject.FromObject(rel.Filter),
                            "rel"
                        );

                        whereParts.Add($"ALL(rel IN relationships({relVar}) WHERE {relFilter})");
                    }
                }
            }

            // =========================
            // NODE FILTER
            // =========================
            if (dsl.Filter != null)
            {
                var filter = dsl.Filter as JObject ?? JObject.FromObject(dsl.Filter);
                var nodeWhere = BuildFilter(filter, alias);
                whereParts.Add(nodeWhere);
            }

            // =========================
            // WHERE
            // =========================
            if (whereParts.Any())
            {
                query.AppendLine("WHERE " + string.Join(" AND ", whereParts));
            }

            // =========================
            // RETURN
            // =========================
            query.AppendLine("RETURN " + string.Join(", ", returnItems));

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
                else if (prop.Name == "$not")
                {
                    var sub = BuildFilter((JObject)prop.Value, alias);
                    parts.Add($"NOT ({sub})");
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
                        "$ne" => $"{alias}.{field} <> ${param}",
                        "$gt" => $"{alias}.{field} > ${param}",
                        "$gte" => $"{alias}.{field} >= ${param}",
                        "$lt" => $"{alias}.{field} < ${param}",
                        "$lte" => $"{alias}.{field} <= ${param}",
                        "$in" => $"{alias}.{field} IN ${param}",
                        "$nin" => $"NOT {alias}.{field} IN ${param}",
                        "$contains" => $"{alias}.{field} CONTAINS ${param}",
                        "$startsWith" => $"{alias}.{field} STARTS WITH ${param}",
                        "$endsWith" => $"{alias}.{field} ENDS WITH ${param}",
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
        // PARAM HANDLER
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
                JTokenType.Null => null,
                _ => token.ToString()
            };
        }
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
