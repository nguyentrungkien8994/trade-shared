using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Database.Neo4j.Responses
{
    public class CypherQuery
    {
        public string Query { get; set; }
        public Dictionary<string, object> Params { get; set; } = new();
    }
}
