using Javirs.Common;
using Javirs.Common.Json;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winner.ReverseProxy
{
    public class DefaultProxyResponseWrapper : IProxyResponseWrapper
    {
        public IMerchant Merchant { get; set; }
        public IGatawayResult Wrap(string bizResult, string code, string message)
        {
            code = string.IsNullOrEmpty(code) ? "0000" : code;
            if (string.IsNullOrEmpty(message) && code != "0000")
            {
                message = "系统错误";
            }
            else if (string.IsNullOrEmpty(message) && code == "0000")
            {
                message = null;
            }
            Dictionary<string, object> data = new Dictionary<string, object>();
            data.Add("retCode", string.IsNullOrEmpty(code) ? (string.IsNullOrEmpty(bizResult) ? "0001" : "0000") : code);
            data.Add("retMsg", message);
            data.Add("timestamp", (long)(new TimeStamp().Seconds));
            data.Add("data", bizResult);
            string cipher = null, sign = null;
            if (!string.IsNullOrEmpty(bizResult))
            {
                cipher = Merchant.Encrypt(bizResult);
                StringBuilder builder = new StringBuilder();
                AsciiSortedDictionary<object> signDictionary = new AsciiSortedDictionary<object>();
                data.Aggregate(signDictionary, (d, kv) =>
                {
                    if (kv.Value != null && !string.IsNullOrEmpty(kv.Value.ToString()))
                    {
                        d.Add(kv.Key, kv.Value);
                    }
                    return d;
                });
                signDictionary.Aggregate(builder, (b, kv) => b.Append(kv.Key).Append("=").Append(kv.Value).Append("&"));
                sign = Merchant.SignData(builder.Remove(builder.Length - 1, 1).ToString());
            }
            data["data"] = cipher;
            data.Add("sign", sign);
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
}
