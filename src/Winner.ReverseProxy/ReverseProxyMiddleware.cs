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
        public ReverseProxyMiddleware(RequestDelegate next, IServiceProvider svp, IOptions<ReverseProxyConfigModel> config)
        {
            this._svp = svp;
            this.ReverseProxyConfig = config?.Value;
        }

        public async Task Invoke(HttpContext context)
        {
            IGatawayResult result = null;
            var wrapper = GetResponseWrapper();

            var reverseContext = GetProxyContext(context);
            reverseContext.ReverseConfig = this.ReverseProxyConfig;
            reverseContext.Merchant = ResolveMerchantInfo();
            wrapper.Merchant = reverseContext.Merchant;
            RequestDataResolver dataResolver = new RequestDataResolver();
            var rlvres = dataResolver.Resolve(reverseContext);
            if (!rlvres.Success)
            {
                result = wrapper.Wrap(null, rlvres.StatusCode.ToString().PadLeft(4, '0'), rlvres.Message);
                await result.ExecuteAsync(context);
                return;
            }
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
        private ReverseProxyContext GetProxyContext(HttpContext context)
        {
            var ctx = new ReverseProxyContext();
            ctx.Query = context.Request.Query;
            ctx.RequestUrl = string.Concat(context.Request.Scheme, "://", context.Request.Host, context.Request.Path, context.Request.QueryString.Value);
            ctx.Cookies = context.Request.Cookies;
            ctx.HttpMethod = context.Request.Method;
            ctx.InputStream = context.Request.Body;
            ctx.RequestContentType = context.Request.ContentType;
            foreach (string k in context.Request.Headers.Keys)
            {
                ctx.Headers.Add(k, context.Request.Headers[k]);
            }
            return ctx;
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
        private IMerchantInfo ResolveMerchantInfo()
        {
            IMerchantInfo merchant = _svp.CreateScope().ServiceProvider.GetService<IMerchantInfo>();
            return merchant ?? new Merchant();
        }
        /// <summary>
        /// 发送代理请求
        /// </summary>
        /// <returns></returns>
        private bool SendProxyRequest(ReverseProxyContext context, out string response)
        {
            try
            {
                var http = new HttpHelper(context.TransferUrl);
                if (context.Headers != null && context.Headers.Count > 0)
                {
                    context.Headers.Aggregate(http, (h, kv) => h.AddHeaderData(kv.Key, kv.Value));
                }
                if ("GET".Equals(context.HttpMethod, StringComparison.OrdinalIgnoreCase))
                {
                    response = http.Get(context.ForwardValue.ToString());
                }
                else
                {
                    response = http.SendRequest(context.HttpMethod, context.ForwardValue.ToArray(), context.ReverseConfig.timeout, false);
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
    public interface IRequestValidate
    {
        FuncResult Validate(ReverseProxyContext context);
    }
    public class RequestDataResolver
    {
        public FuncResult Resolve(ReverseProxyContext context)
        {
            string merchantNo = null, cipherData = null, sign = null;
            ContentType cttType;
            if ("POST".Equals(context.HttpMethod, StringComparison.OrdinalIgnoreCase))
            {
                string postData = null;
                using (StreamReader reader = new StreamReader(context.InputStream))
                {
                    postData = reader.ReadToEnd();
                }
                if (string.IsNullOrEmpty(postData))
                {
                    return FuncResult.FailResult("无效报文", 405);
                }
                JsonObject Jo = JsonObject.Parse(postData);
                merchantNo = Jo.GetString("merchantNo");
                cipherData = Jo.GetString("data");
                sign = Jo.GetString("sign");
                cttType = ContentType.json;
            }
            else if ("GET".Equals(context.HttpMethod, StringComparison.OrdinalIgnoreCase))
            {
                cttType = ContentType.form_urlencoded;
                StringBuilder builder = new StringBuilder();
                context.Query.Aggregate(builder, (b, kv) => b.Append(kv.Key).Append("=").Append(kv.Value).Append("&"));
                cipherData = builder.ToString().Trim('&');
            }
            else
            {
                return FuncResult.FailResult("不支持的请求头", 415);
            }
            context.Merchant.Initialize(merchantNo);
            string data = context.Merchant.Decrypt(cipherData);
            context.ForwardValue = new ProxyForwardValue(data, cttType);
            if (!context.Merchant.VerifySignature(data, sign))
            {
                return FuncResult.FailResult("签名验证不正确", 2);
            }
            return FuncResult.SuccessResult();
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
        IMerchantInfo Merchant { get; set; }
    }
    public class DefaultProxyResponseWrapper : IProxyResponseWrapper
    {
        public IMerchantInfo Merchant { get; set; }
        public IGatawayResult Wrap(string bizResult, string code, string message)
        {
            code = string.IsNullOrEmpty(code) ? "0000" : code;
            if (string.IsNullOrEmpty(message) && code != "0000")
            {
                message = "系统错误";
            }
            else if (string.IsNullOrEmpty(message) && code == "0000")
            {
                message = "ok";
            }
            string cipher = null, sign = null;
            if (!string.IsNullOrEmpty(bizResult))
            {
                cipher = Merchant.Encrypt(bizResult);
                sign = Merchant.SignData(bizResult);
            }
            object data = new
            {
                retCode = string.IsNullOrEmpty(code) ? (string.IsNullOrEmpty(bizResult) ? "0001" : "0000") : code,
                retMsg = message,
                data = cipher,
                sign
            };
            string json = JsonSerializer.JsonSerialize(data);
            return new GatewayResult(json, "application/json");
        }

        public class GatewayResult : IGatawayResult
        {
            private string json;
            private string contentType;
            public GatewayResult(string json, string contentType)
            {
                this.json = json;
                this.contentType = contentType;
            }
            public async Task ExecuteAsync(HttpContext context)
            {
                context.Response.ContentType = this.contentType;
                await context.Response.WriteAsync(this.json);
            }
        }
    }
    public interface IGatawayResult
    {
        Task ExecuteAsync(HttpContext context);
    }

    /// <summary>
    /// 反向代理上下文
    /// </summary>
    public class ReverseProxyContext
    {
        /// <summary>
        /// 链接查询参数
        /// </summary>
        public IQueryCollection Query { get; set; }
        /// <summary>
        /// body流
        /// </summary>
        public Stream InputStream { get; set; }
        /// <summary>
        /// 请求内容类型
        /// </summary>
        public string RequestContentType { get; set; }
        /// <summary>
        /// 请求地址
        /// </summary>
        public string RequestUrl { get; set; }
        /// <summary>
        /// 转发地址
        /// </summary>
        public string TransferUrl { get; set; }
        /// <summary>
        /// 代理转发数据
        /// </summary>
        public ProxyForwardValue ForwardValue { get; set; }
        /// <summary>
        /// http请求方法（get/post）
        /// </summary>
        public string HttpMethod { get; set; }
        /// <summary>
        /// 转发http头
        /// </summary>
        public Dictionary<string, string> Headers { get; set; } = new Dictionary<string, string>();
        /// <summary>
        /// 转发cookie
        /// </summary>
        public IRequestCookieCollection Cookies { get; set; }
        public IMerchantInfo Merchant { get; set; }
        public ReverseProxyConfigModel ReverseConfig { get; set; }
    }
    public class ProxyForwardValue
    {
        private Dictionary<string, string> keyValuePairs;
        private string _requestData;
        private ContentType contentType;
        public ProxyForwardValue(string requestData, ContentType contentType)
        {
            keyValuePairs = new Dictionary<string, string>();
            this._requestData = requestData;
            this.contentType = contentType;
        }
        public void Append(string name, string value)
        {
            keyValuePairs.Add(name, value);
        }
        public override string ToString()
        {
            if (string.IsNullOrEmpty(this._requestData) && keyValuePairs.Count <= 0)
            {
                return null;
            }
            if (this.contentType == ContentType.json)
            {
                JsonObject JObject = JsonObject.Parse(this._requestData);
                foreach (var kv in keyValuePairs)
                {
                    JObject.Put(kv.Key, kv.Value);
                }
                return JObject.ToString();
            }
            else
            {
                StringBuilder builder = new StringBuilder(_requestData);
                keyValuePairs.Aggregate(builder, (b, kv) => b.Append("&").Append(kv.Key).Append("=").Append(kv.Value));
                return builder.ToString().Trim('&');
            }
        }
        public byte[] ToArray()
        {
            string str = this.ToString();
            if (string.IsNullOrEmpty(str))
            {
                return null;
            }
            return Encoding.UTF8.GetBytes(str);
        }
    }

    public interface IMerchantInfo
    {
        IMerchantInfo Initialize(string merchantNo);
        bool VerifySignature(string original, string sign);
        string SignData(string original);
        string Encrypt(string text);
        string Decrypt(string cipher);
    }

    public class Merchant : IMerchantInfo
    {
        public string SignType { get; set; }
        public string MerchantName { get; set; }
        public string MerchantCode { get; set; }
        public string AppSecret { get; set; }
        public string RsaPublicKey { get; set; }
        public string RsaPrivateKey { get; set; }
        public string Decrypt(string cipher)
        {
            //无需解密就直接返回
            return cipher;
        }

        public string Encrypt(string text)
        {
            //若不需要加密就直接返回
            return text;
        }

        public IMerchantInfo Initialize(string merchantNo)
        {
            this.MerchantCode = "2000001019";
            this.MerchantName = "test";
            this.AppSecret = Guid.NewGuid().ToString();
            this.SignType = "MD5";
            return this;
        }

        public string SignData(string original)
        {
            return MD5.Encode(original + AppSecret);
        }

        public bool VerifySignature(string original, string sign)
        {
            return string.Equals(MD5.Encode(original + AppSecret), sign, StringComparison.OrdinalIgnoreCase);
        }
    }
}
