using Core.Database;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Neo4j.Driver;
using Shared.Database.Neo4j.DataAccess;
using Shared.Database.Neo4j.Repository;
using Shared.Database.Neo4j.Service;
using System.Reflection;
namespace Shared.Database.MongoDb
{
    public static class Extensions
    {
        public static void UseDatabaseNeo4j(this IServiceCollection services, string uri, string username, string password)
        {
            services.AddSingleton<IDriver>(sp =>
                GraphDatabase.Driver(uri, AuthTokens.Basic(username, password))
            );
            services.AddScoped(typeof(IDataAccess), typeof(DataAccess));
            services.AddScoped(typeof(IRepositoryBaseNeo4j<,>), typeof(RepositoryBase<,>));
            services.AddScoped(typeof(IServiceBaseNeo4j<,,>), typeof(ServiceBase<,,>));
            services.AddScoped(typeof(IServiceBaseNeo4j<,>), typeof(ServiceBase<,>));
            services.AddScoped(typeof(IServiceBaseNeo4j<>), typeof(ServiceBase<>));
        }
        public static async Task<List<T>> ToNeo4jListAsync<T>(
        this IResultCursor cursor,
        Func<IRecord, T>? customMap = null)
        {
            var list = new List<T>();

            while (await cursor.FetchAsync())
            {
                var record = cursor.Current;

                if (customMap != null)
                {
                    list.Add(customMap(record));
                    continue;
                }

                list.Add(record.ToRecord<T>());
            }

            return list;
        }

        // ===================== CORE MAPPING =====================
        public static T ToRecord<T>(this IRecord record)
        {
            var value = record.Values.Values.FirstOrDefault();

            if (value == null)
                return default!;

            // Case 1: Node
            if (value is INode node)
                return MapNode<T>(node);

            // Case 2: Primitive / scalar
            if (value is T tValue)
                return tValue;

            // Case 3: fallback convert
            return (T)Convert.ChangeType(value, typeof(T));
        }

        private static T MapNode<T>(INode node)
        {
            var entity = Activator.CreateInstance<T>();

            var props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var prop in props)
            {
                if (!prop.CanWrite) continue;

                if (node.Properties.TryGetValue(prop.Name, out var value))
                {
                    try
                    {
                        var converted = ConvertValue(value, prop.PropertyType);
                        prop.SetValue(entity, converted);
                    }
                    catch
                    {
                        // ignore conversion lỗi
                    }
                }
            }

            return entity!;
        }

        private static object? ConvertValue(object value, Type targetType)
        {
            if (value == null) return null;

            // Handle Guid
            if (targetType == typeof(Guid))
                return Guid.Parse(value.ToString()!);

            // Handle DateTime (Neo4j temporal)
            if (value is ZonedDateTime zdt)
                return zdt.ToDateTimeOffset().UtcDateTime;

            if (value is LocalDateTime ldt)
                return ldt.ToDateTime();

            // Handle ObjectId (Mongo style nếu bạn dùng chung)
            if (targetType.Name == "ObjectId")
                return Activator.CreateInstance(targetType, value.ToString());

            return Convert.ChangeType(value, targetType);
        }
    }
}
