using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.SpaServices.VueDevelopmentServer.Npm;
using Microsoft.AspNetCore.SpaServices.VueDevelopmentServer.Util;
using Microsoft.Extensions.Logging;

namespace Microsoft.AspNetCore.SpaServices.VueDevelopmentServer
{
    internal static class VueDevelopmentServerMiddleware
    {
        private const string LogCategoryName = "Microsoft.AspNetCore.SpaServices";
        private static readonly TimeSpan RegexMatchTimeout = TimeSpan.FromSeconds(5.0);

        private static string CompiledSuccessfullyString { get; set; }

        public static void Attach(
            ISpaBuilder spaBuilder,
            string npmScriptName,
            string compiledSuccessfullyString)
        {
            var sourcePath = spaBuilder.Options.SourcePath;
            if (string.IsNullOrEmpty(sourcePath))
                throw new ArgumentNullException(nameof(sourcePath));
            if (string.IsNullOrEmpty(npmScriptName))
                throw new ArgumentNullException(nameof(npmScriptName));
            CompiledSuccessfullyString = compiledSuccessfullyString;
            var logger = LoggerFinder.GetOrCreateLogger(spaBuilder.ApplicationBuilder, LogCategoryName);
            var targetUriTask = StartCreateVueAppServerAsync(sourcePath, npmScriptName, logger)
                .ContinueWith(task => new UriBuilder("http", "127.0.0.1", task.Result).Uri);
            spaBuilder.UseProxyToSpaDevelopmentServer(() =>
            {
                var startupTimeout = spaBuilder.Options.StartupTimeout;
                return targetUriTask.WithTimeout(startupTimeout,
                    $"在 {startupTimeout.Seconds} 秒钟的超时时间内，Vue服务器未开始侦听请求。 检查日志输出以获取错误信息");
            });
        }

        private static async Task<int> StartCreateVueAppServerAsync(
            string sourcePath,
            string npmScriptName,
            ILogger logger)
        {
            var portNumber = TcpPortFinder.FindAvailablePort();
            logger.LogInformation($"在 {portNumber} 端口上启动Vue服务...");
            var npmScriptRunner = new NpmScriptRunner(sourcePath, npmScriptName, null, new Dictionary<string, string>
            {
                {
                    "PORT",
                    portNumber.ToString()
                },
                {
                    "BROWSER",
                    "none"
                }
            });
            npmScriptRunner.AttachToLogger(logger);
            using (var stdErrReader = new EventedStreamStringReader(npmScriptRunner.StdErr))
            {
                try
                {
                    var match = await npmScriptRunner.StdOut.WaitForMatch(new Regex(
                        !string.IsNullOrEmpty(CompiledSuccessfullyString)
                            ? CompiledSuccessfullyString
                            : "DONE  Compiled successfully", RegexOptions.None, RegexMatchTimeout));
                }
                catch (EndOfStreamException ex)
                {
                    throw new InvalidOperationException(
                        "NPM 脚本'" + npmScriptName + "' 已退出,但是Vue服务还没开始侦听请求. 错误输出为: " +
                        stdErrReader.ReadAsString(), ex);
                }
            }

            return portNumber;
        }
    }
}