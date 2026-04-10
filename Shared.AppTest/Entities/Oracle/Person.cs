using Core.Database.Entity;
using Shared.Database.Oracle.Attributes;

namespace Shared.AppTest.Entities.Oracle
{
    [Table("SYS_PERSON")]
    public class Person : IEntityBase<int>
    {
        public int id { get; set; }

        public string created_by { get; set; }

        public string updated_by { get; set; }

        public long created_at { get; set; }

        public long updated_at { get; set; }
        public string FULLNAME { get; set; }
        public string ROLENAME { get; set; }
        public int SYNC_STATUS { get; set; }
    }
}
