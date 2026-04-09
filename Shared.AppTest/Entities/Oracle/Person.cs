using Core.Database.Entity;
using Shared.Database.Oracle.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
}
