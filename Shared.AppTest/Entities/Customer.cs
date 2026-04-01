using Core.Database.Entity;
using MongoDB.Bson;
using Shared.Database.MongoDb.Entity;

namespace Shared.AppTest.Entities
{
    public class Customer : IEntityBase<ObjectId>
    {
        public string code { get; set; }
        public string name { get; set; }

        public ObjectId id { get; set; }

        public string created_by { get; set; }

        public string updated_by { get; set; }

        public long created_at { get; set; }

        public long updated_at { get; set; }
    }
}
