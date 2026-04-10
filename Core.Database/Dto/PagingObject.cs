using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KLib.Core.Database.Dto
{
    public class PagingObject<T>
    {
        public List<T> Data { get; set; }
        public int TotalCount { get; set; }
        public int Skip { get; set; }
        public int Take { get; set; }
    }
}
