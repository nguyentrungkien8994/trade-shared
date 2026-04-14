using KLib.Core.Database.Entity;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shared.AppTest.Entities.Neo4j
{
    [Table("SYS_COMPANY")]
    public class Company : IEntityKey<int>
    {
        public int entityId { get; set; }

        public string created_by { get; set; }

        public string updated_by { get; set; }

        public long created_at { get; set; }

        public long updated_at { get; set; }
        public string TICKER { get; set; }
        public string FOREIGNOWNERSHIPRATIONAME { get; set; }
        public decimal OUTSTANDINGSHARE { get; set; }
        public decimal FOREIGNERVOLUMN { get; set; }
        public decimal FOREIGNOWNERSHIPRATIO { get; set; }
        public decimal OTHERVOLUMN { get; set; }
        public decimal OTHERPERCENTAGE { get; set; }
        public int SYNC_STATUS { get; set; }
    }
}
