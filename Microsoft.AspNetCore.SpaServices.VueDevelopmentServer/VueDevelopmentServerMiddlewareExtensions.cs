using System;

namespace Microsoft.AspNetCore.SpaServices.VueDevelopmentServer
{
    /// <summary>
    /// Spa的Vue项目开发服务扩展
    /// </summary>
    public static class VueDevelopmentServerMiddlewareExtensions
    {
        /// <summary>
        /// 在spa闭包中使用此函数来启动一个vue服务
        /// </summary>
        /// <param name="spaBuilder">Spa服务构建器</param>
        /// <param name="npmScript">npm脚本名称</param>
        /// <param name="compiledSuccessfullyString">用于判断服务启动完成的字符串</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        public static void UseVueDevelopmentServer(
            this ISpaBuilder spaBuilder,
            string npmScript,
            string compiledSuccessfullyString = null)
        {
            if (spaBuilder == null)
                throw new ArgumentNullException(nameof (spaBuilder));
            if (string.IsNullOrEmpty(spaBuilder.Options.SourcePath))
                throw new InvalidOperationException("在使用UseVueDevelopmentServer之前必须在UseSpa闭包中为SpaOptions.SourcePath属性设置一个非空的值.");
            VueDevelopmentServerMiddleware.Attach(spaBuilder, npmScript, compiledSuccessfullyString);
        }
    }
}