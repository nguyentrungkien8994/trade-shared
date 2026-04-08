namespace Shared.Database.Neo4j.Requests
{
    public class SearchParam
    {
        public string Node { get; set; }
        public object Filter { get; set; }
        public List<RelationDsl> Relations { get; set; }
        public Target Target { get; set; }
        
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
    public class GraphQueryOptions
    {
        public int DefaultMinDepth { get; set; } = 1;
        public int DefaultMaxDepth { get; set; } = 10;
        public int HardMaxDepth { get; set; } = 100; // chống abuse
    }
    public class Target
    {
        public string Node {  set; get; }
        public object Filter { get; set; }
    }
}
