using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Redis
{
    public class RedisOptions
    {
        public string ConnectionString { get; set; } = default!;
        public string InstanceName { get; set; } = "app:";
    }
}
