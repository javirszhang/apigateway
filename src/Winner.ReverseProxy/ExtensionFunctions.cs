using Microsoft.AspNetCore.Builder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Winner.ReverseProxy;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ExtensionFunctions
    {
        public static void AddMerchantInfo<T>(this IServiceCollection services) where T : class, IMerchant
        {
            services.AddScoped<IMerchant, T>();
        }
        public static void AddRequestValidation<T>(this IServiceCollection services) where T : class, IRequestValidate
        {
            services.AddScoped<IRequestValidate, T>();
        }

        public static void UseDefaultGateway(this IApplicationBuilder app)
        {
            app.UseMiddleware<ReverseProxyMiddleware>();
        }
    }
}
