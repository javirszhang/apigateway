using System;
using System.Collections.Generic;
using System.Text;

namespace Winner.ReverseProxy.Gateway.Interfaces
{
    public interface IParameterModel
    {
        string GetToken();
        IMerchant GetMerchant();
        string GetRequestSignature();
        bool VerifySignature();
        string GetApiVersion();
        string GetServiceCode();
        string GetBizContent();
    }
}
