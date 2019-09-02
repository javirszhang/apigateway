using System;
using System.Collections.Generic;
using System.Text;
using Winner.ReverseProxy.Gateway.Interfaces;

namespace Winner.ReverseProxy.Gateway
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
            return new Merchant();
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

        public bool VerifySignature()
        {
            return true;
        }
    }
}
