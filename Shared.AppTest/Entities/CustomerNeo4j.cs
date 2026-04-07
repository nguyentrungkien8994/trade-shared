
using Shared.Database.Neo4j.Entity;

namespace Shared.AppTest.Entities
{
    public class CustomerNeo4j: EntityBase<string>
    {
        public string code { get; set; }
        public string name { get; set; }
    }
}
