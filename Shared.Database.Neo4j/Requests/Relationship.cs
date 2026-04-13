namespace Shared.Database.Neo4j.Requests
{
    public class Relationship
    {
        public string FromNode { get; set; }
        public object FromId { get; set; }
        public string ToNode { get; set; }
        public object ToId { get; set; }
        public string RelationName { get; set; }
    }
}
