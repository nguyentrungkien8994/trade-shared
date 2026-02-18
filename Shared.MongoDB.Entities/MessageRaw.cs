using Shared.MongoDB.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.MongoDB.Entities;

public class MessageRaw:EntityBase
{

    public string msg_id { get; set; }
    public string raw_msg { get; set; }
    public string[] imgs { get; set; }
    public long processed_at { get; set; }
    public string source { get; set; }
    public string channel { get; set; }
    public string owner { get; set; }
}
