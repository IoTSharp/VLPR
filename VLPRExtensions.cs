using System;
using System.Text.RegularExpressions;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class VLPRExtensions
    {
        /// <summary>
        /// 加入车牌识别服务
        /// </summary>
        /// <param name="services"></param>
        public static void AddVPRService(this  IServiceCollection services)
        {
            services.AddOptions<VLPROptions>().BindConfiguration(nameof(VLPROptions));
            services.AddHostedService<VLPRService>();
            services.AddSingleton<VLPRClient>();
        
        }
        /// <summary>
        /// 加入健康检查
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static IHealthChecksBuilder AddVLPR(this IHealthChecksBuilder builder,string name=null)
        {
            return builder.AddTypeActivatedCheck<VLPRHealthCheck>(name??"VLPR");
        }
    }
}
namespace System.Text
{
    internal static class StringExtensions
    {
        internal static string RemoveNull(this string str)
        {
            return Regex.Replace(str, @"[\x01-\x1F,\x7F,' ','\0']", ""); 
        }
    }
}