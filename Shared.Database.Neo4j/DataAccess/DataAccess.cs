using Neo4j.Driver;
using Neo4j.Driver.Mapping;
using Shared.Database.MongoDb;

namespace Shared.Database.Neo4j.DataAccess
{
    public class DataAccess : IDataAccess
    {
        private readonly IDriver _driver;
        public DataAccess(IDriver driver)
        {
            _driver = driver;
        }
        public async Task<IEnumerable<T>> ReadAsync<T>(string cypher, object? parameters = null)
        {
            await using var session = _driver.AsyncSession(o =>
            o.WithDefaultAccessMode(AccessMode.Read));

            return await session.ExecuteReadAsync(async tx =>
            {
                var cursor = await tx.RunAsync(cypher, parameters);
                return await cursor.ToNeo4jListAsync<T>();
            });
        }

        public async Task<IEnumerable<IDictionary<string, object>>> ReadAsync(string cypher, object? parameters = null)
        {
            await using var session = _driver.AsyncSession(o =>
             o.WithDefaultAccessMode(AccessMode.Read));

            return await session.ExecuteReadAsync(async tx =>
            {
                var cursor = await tx.RunAsync(cypher, parameters);
                return await cursor.ToListDictionaryAsync();
            });
        }

        public async Task<IEnumerable<IRecord>> ReadMultipleNodeAsync(string cypher, object? parameters = null)
        {
            await using var session = _driver.AsyncSession(o =>
            o.WithDefaultAccessMode(AccessMode.Read));

            return await session.ExecuteReadAsync(async tx =>
            {
                var cursor = await tx.RunAsync(cypher, parameters);
                return await cursor.ToListAsync();
            });
        }

        public async Task<T?> WriteAsync<T>(string cypher, object? parameters = null)
        {
            await using var session = _driver.AsyncSession(o =>
            o.WithDefaultAccessMode(AccessMode.Write));

            return await session.ExecuteWriteAsync<T?>(async tx =>
             {
                 var cursor = await tx.RunAsync(cypher, parameters);
                 var records = await cursor.ToListAsync();
                 if (records.Any()) return records[0].ToRecord<T>();
                 return default;
             });
        }
        public async Task<IEnumerable<IRecord>> WriteAsync(string cypher, object? parameters = null)
        {
            await using var session = _driver.AsyncSession(o =>
            o.WithDefaultAccessMode(AccessMode.Write));

            return await session.ExecuteWriteAsync(async tx =>
             {
                 var cursor = await tx.RunAsync(cypher, parameters);
                 return await cursor.ToListAsync();
             });
        }

        public async Task<IResultSummary> WriteScalarAsync(string cypher, object? parameters)
        {
            await using var session = _driver.AsyncSession(o =>
            o.WithDefaultAccessMode(AccessMode.Write));

            return await session.ExecuteWriteAsync(async tx =>
            {
                var cursor = await tx.RunAsync(cypher, parameters);
                return await cursor.ConsumeAsync();
            });
        }
    }
}
