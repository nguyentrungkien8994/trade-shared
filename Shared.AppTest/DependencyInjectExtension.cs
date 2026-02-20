using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shared.Kafka;

namespace Shared.AppTest
{
    public static class DependencyInjectExtension
    {
        /// <summary>
        /// add confluent kafka connection
        /// </summary>
        /// <param name="services"></param>
        public static void AddConfluentKafka(this IServiceCollection services, string bootstrap, string groupId, string topic) {
            // Kafka options
            services.AddSingleton<IOptions<KafkaOptions>>(
                Options.Create(new KafkaOptions
                {
                    BootstrapServers = bootstrap,
                    GroupId = groupId,
                    Topic = topic
                })
            );
            services.AddSingleton<IKafkaConsumer, KafkaConsumer>();
            services.AddSingleton<IKafkaProducer, KafkaProducer>();
        }

        ///// <summary>
        ///// add mongodb connection
        ///// </summary>
        ///// <param name="services"></param>
        //public static void AddMongoDB(this IServiceCollection services,string uri,string database)
        //{
        //    // Kafka options
        //    services.AddSingleton<IOptions<MongoSettings>>(
        //        Options.Create(new MongoSettings
        //        {
        //            ConnectionString = uri,
        //            Database= database
        //        })
        //    );
        //    services.AddSingleton<IMongoClient>(sp =>
        //    {
        //        var settings = sp.GetRequiredService<IOptions<MongoSettings>>().Value;
        //        return new MongoClient(settings.ConnectionString);
        //    });
        //    services.AddSingleton<MongoDBContext>();
        //}

        ///// <summary>
        ///// add nlog
        ///// </summary>
        ///// <param name="services"></param>
        //public static void AddNLog(this IServiceCollection services)
        //{
        //    services.AddLogging(builder => {
        //        builder.ClearProviders();
        //        builder.AddNLog(NLogConfiguration.GetConfig());
        //    });

        //}
        //public static void AddBusinessService(this IServiceCollection services)
        //{
        //    services.AddScoped(typeof(IRepositoryBase<>), typeof(RepositoryBase<>));
        //    services.AddScoped(typeof(IServiceBase<>), typeof(ServiceBase<>));
        //}
    }
}
