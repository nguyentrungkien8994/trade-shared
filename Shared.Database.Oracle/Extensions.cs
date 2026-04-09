using Core.Database;
using Microsoft.Extensions.DependencyInjection;
using Shared.Database.Oracle.Builder;
using Shared.Database.Oracle.Factory;
using Shared.Database.Oracle.Repository;
using Shared.Database.Oracle.Service;
namespace Shared.Database.MongoDb
{
    public static class Extensions
    {
        public static IServiceCollection AddOracle(
        this IServiceCollection services,
        string connectionString)
        {
            // ================= CORE =================
            services.AddSingleton<IOracleSqlBuilder, OracleSqlBuilder>();

            // ================= CONNECTION =================
            services.AddScoped<IOracleConnectionFactory>(_ =>
                new OracleConnectionFactory(connectionString));

            // ================= REPOSITORY =================
            services.AddScoped(typeof(IRepositoryBase<,>), typeof(RepositoryBase<,>));

            // ================= SERVICE BASE =================
            services.AddScoped(typeof(IServiceBase<,,>), typeof(ServiceBase<,,>));
            services.AddScoped(typeof(IServiceBase<,>), typeof(ServiceBase<,>));
            services.AddScoped(typeof(IServiceBase<>), typeof(ServiceBase<>));

            return services;
        }
    }
}
