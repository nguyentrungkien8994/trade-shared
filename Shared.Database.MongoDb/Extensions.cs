using Core.Database;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Shared.Database.MongoDb.Service;
using Core.Database;
using Shared.Database.MongoDb.Repository;
namespace Shared.Database.MongoDb
{
    public static class Extensions
    {
        public static void UseDatabaseMongoDb(this IServiceCollection services, string uri, string database)
        {
            // Kafka options
            services.AddSingleton<IOptions<MongoSettings>>(
                Options.Create(new MongoSettings
                {
                    ConnectionString = uri,
                    Database = database
                })
            );
            services.AddSingleton<IMongoClient>(sp =>
            {
                var settings = sp.GetRequiredService<IOptions<MongoSettings>>().Value;
                return new MongoClient(settings.ConnectionString);
            });
            services.AddSingleton<MongoDBContext>();
            services.AddScoped(typeof(IRepositoryBase<,>), typeof(RepositoryBase<,>));
            services.AddScoped(typeof(IServiceBase<,,>), typeof(ServiceBase<,,>));
            services.AddScoped(typeof(IServiceBase<,>), typeof(ServiceBase<,>));
            services.AddScoped(typeof(IServiceBase<>), typeof(ServiceBase<>));
        }
    }
}
