using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Redis.OM;
using System.Runtime.InteropServices;
using WatchLogger.Exceptions;
using WatchLogger.Helpers;
using WatchLogger.HostedService;

namespace WatchLogger
{
    // Extension method and Class used to add the middleware to the HTTP request pipeline.

    public static class WatchLoggerExtensions
    {
        public static IApplicationBuilder UseWatchLogger(this IApplicationBuilder builder)
        {
            builder.UseMvc();
            return builder.UseMiddleware<WatchLogger>();
        }

        public static IApplicationBuilder UseWatchLoggerExceptionLogger(this IApplicationBuilder builder)
        {

            return builder.UseMiddleware<WatchLoggerExceptionLogger>();
        }

        public static IServiceCollection AddWatchLoggerServices(this IServiceCollection services, [Optional] Action<WatchLogConfig> configureOptions)
        {
            var options = new WatchLogConfig();
            if (configureOptions != null)
                configureOptions(options);

            WatchLogExternalDbConfig.ConnectionString = options.SetExternalDbConnString;

            if (string.IsNullOrEmpty(WatchLogExternalDbConfig.ConnectionString))
                throw new WatchLogDBException("Missing connection string.");


            /* ----- Add Controller & UI Service ---  */

            // services.AddMvc().AddApplicationPart(typeof(WatchLoggerExtensions).Assembly);
            services.AddMvcCore(x =>
            {
                x.EnableEndpointRouting = false;
            }).AddApplicationPart(typeof(WatchLoggerExtensions).Assembly);

            services.AddSingleton(new RedisConnectionProvider(WatchLogExternalDbConfig.ConnectionString));

            // Create Redis Index
            services.AddHostedService<IndexCreationService>();
            services.AddSingleton(new RedisStackHelper());

            return services;
        }


    }
}
