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
        public object ParserRecords<T>(IEnumerable<IRecord> records)
        {
            var nodes = new Dictionary<string, object>();
            var edges = new List<object>();
            var edgeSet = new HashSet<string>();

            foreach (var record in records)
            {
                foreach (var key in record.Keys)
                {
                    var value = record[key];
                    Extract<T>(value, nodes, edges, edgeSet);
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
        private void Extract<T>(object value,
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
                    Extract<T>(n, nodes, edges, edgeSet);

                foreach (var r in path.Relationships)
                    Extract<T>(r, nodes, edges, edgeSet);

                return;
            }

            // =========================
            // LIST / COLLECTION
            // =========================
            if (value is IEnumerable<object> list)
            {
                foreach (var item in list)
                {
                    Extract<T>(item, nodes, edges, edgeSet);
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
                    Extract<T>(v, nodes, edges, edgeSet);
                }

                return;
            }

            //T type
            //if (value is T)
            //{
            //    var id = Guid.NewGuid().ToString();

            //    if (!nodes.ContainsKey(id))
            //    {
            //        nodes[id] = value;
            //    }

            //    return;
            //}
        }
    }
}
