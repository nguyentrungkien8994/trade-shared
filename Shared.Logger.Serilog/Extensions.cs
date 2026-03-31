using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace Shared.Logger.Serilog
{
    public static class Extensions
    {
        // <summary>
        /// add serilog
        /// </summary>
        /// <param name="services"></param>
        public static void AddSerilog(this IServiceCollection services,IConfiguration configuration)
        {
            //Log.Logger = new LoggerConfiguration()
            //            .ReadFrom.Configuration(configuration)
            //            .CreateLogger();
            //services.AddLogging(builder =>
            //{
            //    builder.ClearProviders();
            //    builder.AddSerilog(Log.Logger);
            //});
            services.AddSerilog((services, loggerConfiguration) =>
            {
                loggerConfiguration
                    .ReadFrom.Configuration(configuration);
            });
        }
    }
}
