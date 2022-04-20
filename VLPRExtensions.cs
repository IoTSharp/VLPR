using Microsoft.AspNetCore.Builder;
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
            services.AddHostedService<VLPRService>();
            services.AddOptions<VLPROptions>();
            services.AddSingleton<VehicleQueue>();
        }
       
        /// <summary>
        /// 通过依赖注入取得 VehicleQueue queue ，用TryTake 来获取新车牌。 
        /// </summary>
        /// <param name="app"></param>
        public static void UseVLPRByQueue(this IApplicationBuilder app)
        {
            app.ApplicationServices.GetService<VLPRService>().SetQueue();
        }
    }
}