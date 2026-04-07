namespace Shared.Database.Neo4j.Requests
{
    public class SearchParam
    {
        public string Node { get; set; }
        public object Filter { get; set; }
        public List<RelationDsl> Relations { get; set; }
    }
    public class RelationDsl
    {
        public string Type { get; set; }
        public string Direction { get; set; } // out | in | both
        public string Target { get; set; }
        public DepthDsl Depth { get; set; }
        public object Filter { get; set; }
    }

    public class DepthDsl
    {
        public int Min { get; set; }
        public int Max { get; set; }
    }
}
