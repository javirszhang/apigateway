using Javirs.Common;
using Javirs.Common.Json;
using System;
using System.Collections.Generic;
using System.Text;
using Winner.ReverseProxy.Gateway.Interfaces;
using Winner.ReverseProxy.Gateway;
using Winner.Framework.Utils.Model;

namespace Winner.Gateway.DataResolver
{
    public class GatewayParameterModel : IParameterModel
    {
        public string MerchantNo { get; set; }
        public long Timestamp { get; set; }
        public string Service { get; set; }
        public string ApiVersion { get; set; }
        public string Token { get; set; }
        public string Data { get; set; }
        public string Sign { get; set; }
        public string RequestOriginalString { get; set; }
        public string GetApiVersion()
        {
            return this.ApiVersion;
        }

        public string GetBizContent()
        {
            return GetMerchant().Decrypt(Data);
        }

        public IMerchant GetMerchant()
        {
            return MerchantCache.GetMerchant(this.MerchantNo);
        }

        public string GetRequestOriginalString()
        {
            return RequestOriginalString;
        }

        public string GetRequestSignature()
        {
            return this.Sign;
        }

        public string GetServiceCode()
        {
            return this.Service;
        }

        public string GetToken()
        {
            return this.Token;
        }

        public FuncResult VerifySignature()
        {
            IMerchant merchant = GetMerchant();
            if (merchant == null)
            {
                return FuncResult.FailResult("无效商户号", 400);
            }
            AsciiSortedDictionary<object> dic = JsonSerializer.Deserializer<AsciiSortedDictionary<object>>(GetRequestOriginalString());
            dic.Remove(item => item.Key.Equals("sign", StringComparison.OrdinalIgnoreCase) || item.Value == null || string.IsNullOrEmpty(item.Value.ToString()));
            StringBuilder builder = new StringBuilder();
            dic.Aggregate(builder, (b, kv) => b.Append(kv.Key).Append("=").Append(kv.Value).Append("&"));
            builder.Remove(builder.Length - 1, 1);
            bool res = merchant.VerifySignature(builder.ToString(), GetRequestSignature());
            return new FuncResult { Success = res, Message = "签名验证不正确", StatusCode = 2 };
        }
    }
}
