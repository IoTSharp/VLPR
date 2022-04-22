using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;


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