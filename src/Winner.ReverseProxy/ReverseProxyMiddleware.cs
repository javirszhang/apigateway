﻿using Javirs.Common;
using Javirs.Common.Json;
using Javirs.Common.Net;
using Javirs.Common.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winner.Framework.Utils;
using Winner.Framework.Utils.Model;

namespace Winner.ReverseProxy
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
            Log.Info("gateway.do");
            if (this.ReverseProxyConfig.excludePages.Contains(context.Request.Path.Value.ToLower()))
            {
                await next.Invoke(context);
                return;
            }
            IGatawayResult result = null;
            var wrapper = GetResponseWrapper();
            var reverseContext = GetProxyContext(context, this.ReverseProxyConfig);

            RequestDataResolver dataResolver = GetDataResolver();
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
                result = wrapper.Wrap(null, "0503", "系统错误");
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
            RequestDataResolver resolver = _svp.GetService<RequestDataResolver>();
            return resolver ?? new RequestDataResolver();
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

    public interface IGatawayResult
    {
        Task ExecuteAsync(HttpContext context);
    }

    public class Merchant : IMerchant
    {
        public string SignType { get; set; }
        public string MerchantName { get; set; }
        public string MerchantCode { get; set; }
        public string AppSecret { get; set; }
        public string RsaPublicKey { get; set; }
        public string RsaPrivateKey { get; set; }
        public string Decrypt(string cipher)
        {
            if (string.IsNullOrEmpty(cipher))
            {
                return null;
            }
            return Encoding.UTF8.GetString(Base58.Decode(cipher));
        }

        public string Encrypt(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return null;
            }
            return Base58.Encode(Encoding.UTF8.GetBytes(text));
        }

        public string SignData(string original)
        {
            return MD5.Encode(original + AppSecret);
        }
    }
}
