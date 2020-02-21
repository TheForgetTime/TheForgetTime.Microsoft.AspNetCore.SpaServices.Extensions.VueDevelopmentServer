using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Microsoft.AspNetCore.SpaServices.VueDevelopmentServer.Util
{
    internal static class LoggerFinder
    {
        public static ILogger GetOrCreateLogger(
            IApplicationBuilder appBuilder,
            string logCategoryName)
        {
            var service = appBuilder.ApplicationServices.GetService<ILoggerFactory>();
            return service != null ? service.CreateLogger(logCategoryName) : NullLogger.Instance;
        }
    }
}