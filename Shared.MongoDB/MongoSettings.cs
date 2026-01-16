using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.MongoDB
{
    public class MongoSettings
    {
        public string ConnectionString { get; set; } = default!;
        public string Database { get; set; } = default!;
    }
}
