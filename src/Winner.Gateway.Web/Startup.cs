﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Winner.ReverseProxy.Gateway;

namespace Winner.Gateway.Web
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            this.Configuration = configuration;
        }
        protected IConfiguration Configuration { get; set; }
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<RequestDataResolver, Gateway.DataResolver.RequestDataResolver>();
            services.AddReverseProxyConfig(this.Configuration);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            //app.Use(async (context, task) =>
            //{
            //    ConnectionStringSettings conn = ConfigurationManager.ConnectionStrings["Winner.Framework.Oracle.ConnectionString"];
            //    await context.Response.WriteAsync($"current connection string is [{conn.ConnectionString}]");
            //});
            app.UseMiddleware<ReverseProxyMiddleware>();
            //app.Use(async (context, task) =>
            //{
            //    await pipeline.Invoke(conext);
            //});
        }
    }
}
