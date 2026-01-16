using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace Shared.MongoDB
{
    public class MongoDBContext
    {
        private readonly IMongoDatabase _database;

        public MongoDBContext(IMongoClient client, IOptions<MongoSettings> settings)
        {
            var opt = settings.Value;
            _database = client.GetDatabase(opt.Database);
        }

        public IMongoCollection<T> GetCollection<T>(string name)
            => _database.GetCollection<T>(name);
    }
}
