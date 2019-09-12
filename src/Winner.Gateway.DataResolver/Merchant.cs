using Javirs.Common.Security;
using System;
using System.Collections.Generic;
using System.Text;
using Winner.ReverseProxy.Gateway.Interfaces;

namespace Winner.Gateway.DataResolver
{
    public class Merchant : IMerchant
    {
        public int Id { get; set; }
        public string MerchantName { get; set; }
        public string MerchantCode { get; set; }
        public string SignType { get; set; }
        public string SecretKey { get; set; }
        public string GatewayKey { get; set; }
        public string Decrypt(string cipher)
        {
            return cipher;
        }

        public string Encrypt(string text)
        {
            return text;
        }

        public string SignData(string original)
        {
            return MD5.Encode(original + SecretKey);
        }

        public bool VerifySignature(string original, string sign)
        {
            string mSign = MD5.Encode(original + "&key=" + SecretKey);
            return mSign.Equals(sign, StringComparison.OrdinalIgnoreCase);
        }
    }
}
