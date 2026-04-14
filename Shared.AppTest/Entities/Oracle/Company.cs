using KLib.Core.Database.Entity;
using Shared.Database.Oracle.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.AppTest.Entities.Oracle
{
    [Table("SYS_COMPANY")]
    public class Company : IEntityKey<int>
    {
        public int entityId { get; set; }
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
