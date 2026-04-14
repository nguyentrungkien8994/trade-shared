using KLib.Core.Database;
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
            services.AddScoped(typeof(IRepositoryBaseOracle<,>), typeof(RepositoryBaseOracle<,>));
            services.AddScoped(typeof(IRepositoryBaseOracle), typeof(RepositoryBaseOracle));

            // ================= SERVICE BASE =================
            services.AddScoped(typeof(IServiceBaseOracle<,,>), typeof(ServiceBaseOracle<,,>));
            services.AddScoped(typeof(IServiceBaseOracle<,>), typeof(ServiceBaseOracle<,>));
            services.AddScoped(typeof(IServiceBaseOracle<>), typeof(ServiceBaseOracle<>));
            services.AddScoped(typeof(IServiceBaseOracle), typeof(ServiceBaseOracle));

            return services;
        }
    }
}
