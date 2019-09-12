using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Winner.Framework.Utils;
using Winner.ReverseProxy.Gateway.Interfaces;
using Javirs.Common.Security;
using Javirs.Common.Net;

namespace Winner.ReverseProxy.Gateway
{
    /// <summary>
    /// 反向代理中间件
    /// </summary>
    public class ReverseProxyMiddleware
    {
        private IServiceProvider _svp;
        private ReverseProxyConfigModel ReverseProxyConfig;
        private RequestDelegate next;
        public ReverseProxyMiddleware(RequestDelegate next, IServiceProvider svp, IOptions<ReverseProxyConfigModel> config)
        {
            this._svp = svp;
            this.ReverseProxyConfig = config?.Value;
            this.next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            Log.Info("url path = " + context.Request.Path);
            if (this.ReverseProxyConfig.excludePages.Contains(context.Request.Path.Value.ToLower()) ||
                (!string.IsNullOrEmpty(this.ReverseProxyConfig.fixedPage) && !this.ReverseProxyConfig.fixedPage.Equals(context.Request.Path.Value, StringComparison.OrdinalIgnoreCase)))
            {
                await next.Invoke(context);
                return;
            }
            IGatawayResult result = null;
            var wrapper = GetResponseWrapper();
            var reverseContext = GetProxyContext(context, this.ReverseProxyConfig);

            RequestDataResolver dataResolver = GetDataResolver();
            if (dataResolver == null)
            {
                result = wrapper.Wrap(null, "0003", "网关配置错误");
                await result.ExecuteAsync(context);
                return;
            }
            var rlvres = dataResolver.Resolve(reverseContext);
            if (!rlvres.Success)
            {
                result = wrapper.Wrap(null, rlvres.StatusCode.ToString().PadLeft(4, '0'), rlvres.Message);
                await result.ExecuteAsync(context);
                return;
            }
            wrapper.Merchant = reverseContext.Merchant = rlvres.Content.GetMerchant();
            var validateCollection = this.GetValidateCollection();
            if (validateCollection != null && validateCollection.Count() > 0)
            {
                foreach (var validateObject in validateCollection)
                {
                    var validRes = validateObject.Validate(reverseContext);
                    if (!validRes.Success)
                    {
                        result = wrapper.Wrap(null, validRes.StatusCode.ToString().PadLeft(4, '0'), validRes.Message);
                        await result.ExecuteAsync(context);
                        return;
                    }
                }
            }
            ReverseProxyServiceMapping serviceMapping = new ReverseProxyServiceMapping();
            if (!serviceMapping.Resolve(reverseContext))
            {
                result = wrapper.Wrap(null, "0400", "服务不存在");
                await result.ExecuteAsync(context);
                return;
            }

            if (!SendProxyRequest(reverseContext, out string response))
            {
                result = wrapper.Wrap(null, "0503", $"系统错误[{response}]");
                await result.ExecuteAsync(context);
                return;
            }
            result = wrapper.Wrap(response, null, null);
            await result.ExecuteAsync(context);
        }
        private ReverseProxyContext GetProxyContext(HttpContext context, ReverseProxyConfigModel config)
        {
            var ctx = new ReverseProxyContext();
            ctx.Query = context.Request.Query;
            ctx.RequestUrl = string.Concat(context.Request.Scheme, "://", context.Request.Host, context.Request.Path, context.Request.QueryString.Value);
            ctx.Cookies = context.Request.Cookies;
            ctx.HttpMethod = context.Request.Method;
            ctx.InputStream = context.Request.Body;
            ctx.RequestContentType = context.Request.ContentType;
            ctx.ReverseConfig = config;
            foreach (string k in context.Request.Headers.Keys)
            {
                ctx.Headers.Add(k, context.Request.Headers[k]);
            }
            return ctx;
        }
        private RequestDataResolver GetDataResolver()
        {
            RequestDataResolver resolver = (RequestDataResolver)_svp.GetService(typeof(RequestDataResolver));
            return resolver;
        }
        private IProxyResponseWrapper GetResponseWrapper()
        {
            var wrapper = (IProxyResponseWrapper)_svp.GetService(typeof(IProxyResponseWrapper));
            return wrapper ?? new DefaultProxyResponseWrapper();
        }
        private IEnumerable<IRequestValidate> GetValidateCollection()
        {
            List<IRequestValidate> validateObject = new List<IRequestValidate>();
            IEnumerable<IRequestValidate> list = (IEnumerable<IRequestValidate>)_svp.CreateScope().ServiceProvider.GetServices(typeof(IRequestValidate));
            if (list != null)
            {
                validateObject.AddRange(list);
            }
            return validateObject;
        }
        /// <summary>
        /// 发送代理请求
        /// </summary>
        /// <returns></returns>
        private bool SendProxyRequest(ReverseProxyContext context, out string response)
        {
            try
            {
                //remove content-length and host
                context.Headers.Remove(kv => kv.Key.Equals("Content-Length", StringComparison.OrdinalIgnoreCase) || kv.Key.Equals("host", StringComparison.OrdinalIgnoreCase));
                var http = new HttpHelper(context.TransferUrl);
                if (context.Headers != null && context.Headers.Count > 0)
                {
                    context.Headers.Aggregate(http, (h, kv) => { Log.Info($"{kv.Key}={kv.Value}"); return h.AddHeaderData(kv.Key, kv.Value); });
                }
                if ("GET".Equals(context.HttpMethod, StringComparison.OrdinalIgnoreCase))
                {
                    response = http.Get(context.ForwardValue.ToString());
                }
                else
                {
                    //response = http.SendRequest(context.HttpMethod, context.ForwardValue.ToArray(), context.ReverseConfig.timeout, false,"application/json");
                    response = http.Post(context.ForwardValue.ToString(), context.ReverseConfig.timeout, false, "application/json");
                }
                return http.StatusCode == 200;
            }
            catch (Exception ex)
            {
                Log.Error("请求转发失败", ex);
                response = "请求转发失败";
                return false;
            }
        }
    }


    public enum ContentType
    {
        form_urlencoded,
        json
    }
    public interface IProxyResponseWrapper
    {
        IGatawayResult Wrap(string bizResult, string code, string message);
        IMerchant Merchant { get; set; }
    }
}
