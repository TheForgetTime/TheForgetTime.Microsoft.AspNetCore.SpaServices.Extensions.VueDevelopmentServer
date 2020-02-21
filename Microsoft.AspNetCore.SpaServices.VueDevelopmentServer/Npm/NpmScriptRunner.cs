using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.SpaServices.VueDevelopmentServer.Util;
using Microsoft.Extensions.Logging;

namespace Microsoft.AspNetCore.SpaServices.VueDevelopmentServer.Npm
{
    internal class NpmScriptRunner
    {
        private static readonly Regex AnsiColorRegex =
            new Regex("\x001B\\[[0-9;]*m", RegexOptions.None, TimeSpan.FromSeconds(1.0));

        public NpmScriptRunner(
            string workingDirectory,
            string scriptName,
            string arguments,
            IDictionary<string, string> envVars)
        {
            if (string.IsNullOrEmpty(workingDirectory))
                throw new ArgumentException("不能为空.", nameof(workingDirectory));
            if (string.IsNullOrEmpty(scriptName))
                throw new ArgumentException("不能为空.", nameof(scriptName));
            var fileName = "npm";
            var str = "run " + scriptName + " -- " + (arguments ?? string.Empty);
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                fileName = "cmd";
                str = "/c npm " + str;
            }

            var startInfo = new ProcessStartInfo(fileName)
            {
                Arguments = str,
                UseShellExecute = false,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                WorkingDirectory = workingDirectory
            };
            if (envVars != null)
                foreach (var envVar in envVars)
                    startInfo.Environment[envVar.Key] = envVar.Value;
            var process = LaunchNodeProcess(startInfo);
            StdOut = new EventedStreamReader(process.StandardOutput);
            StdErr = new EventedStreamReader(process.StandardError);
        }

        public EventedStreamReader StdOut { get; }

        public EventedStreamReader StdErr { get; }

        public void AttachToLogger(ILogger logger)
        {
            StdOut.OnReceivedLine += (EventedStreamReader.OnReceivedLineHandler) (line =>
            {
                if (string.IsNullOrWhiteSpace(line))
                    return;
                logger.LogInformation(StripAnsiColors(line));
            });
            StdErr.OnReceivedLine += (EventedStreamReader.OnReceivedLineHandler) (line =>
            {
                if (string.IsNullOrWhiteSpace(line))
                    return;
                logger.LogError(StripAnsiColors(line));
            });
            StdErr.OnReceivedChunk += (EventedStreamReader.OnReceivedChunkHandler) (chunk =>
            {
                if (Array.IndexOf(chunk.Array, '\n', chunk.Offset, chunk.Count) >= 0)
                    return;
                Console.Write(chunk.Array, chunk.Offset, chunk.Count);
            });
        }

        private static string StripAnsiColors(string line)
        {
            return AnsiColorRegex.Replace(line, string.Empty);
        }

        private static Process LaunchNodeProcess(ProcessStartInfo startInfo)
        {
            try
            {
                var process = Process.Start(startInfo);
                process.EnableRaisingEvents = true;
                return process;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    "'npm' 启动失败. 要解决此问题:.\n\n[1] 请检查'npm'是否安装,并且添加到环境变量PATH中.\n    当前PATH环境变量为: " +
                    Environment.GetEnvironmentVariable("PATH") +
                    "\n    确保可执行文件在这些目录之一中，或更新您的PATH.\n\n[2] 有关原因的更多详细信息，请参见InnerException.",
                    ex);
            }
        }
    }
}