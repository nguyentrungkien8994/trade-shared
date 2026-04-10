using KLib.Core.Database.Entity;
using MongoDB.Bson;
using Shared.Database.MongoDb.Entity;

namespace Shared.AppTest.Entities
{
    public class Customer : EntityBase
    {
        public string code { get; set; }
        public string name { get; set; }
    }
}
