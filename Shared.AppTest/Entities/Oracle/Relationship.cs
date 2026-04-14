using KLib.Core.Database.Entity;
using Shared.Database.Oracle.Attributes;

namespace Shared.AppTest.Entities.Oracle
{
    [Table("SYS_RELATIONSHIP")]
    public class Relationship : IEntityKey<int>
    {
        public string FNODE { get; set; }
        public object FID { get; set; }
        public string TNODE { get; set; }
        public object TID { get; set; }
        public int SYNC_STATUS { get; set; }
        public string RELATION_NAME { get; set; }

        public int entityId { get; set; }

        public string created_by { get; set; }

        public string updated_by { get; set; }

        public long created_at { get; set; }

        public long updated_at { get; set; }
    }
}
