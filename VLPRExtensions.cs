using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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