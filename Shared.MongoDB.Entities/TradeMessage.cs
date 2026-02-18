using Shared.MongoDB.Entity;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shared.MongoDB.Entities;

//[Table("trade-message")]
public class TradeMessage : EntityBase
{
    public string msg_id { get; set; }
    public string raw_msg { get; set; }
    public string[] imgs { get; set; }
    public long processed_at { get; set; }
    public string source { get; set; }
    public string channel { get; set; }
    public string owner { get; set; }
    public string? trade_json {  get; set; }
}
