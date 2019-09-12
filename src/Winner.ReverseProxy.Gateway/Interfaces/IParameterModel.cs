using System;
using System.Collections.Generic;
using System.Text;
using Winner.Framework.Utils.Model;

namespace Winner.ReverseProxy.Gateway.Interfaces
{
    public interface IParameterModel
    {
        string GetToken();
        IMerchant GetMerchant();
        string GetRequestSignature();
        FuncResult VerifySignature();
        string GetApiVersion();
        string GetServiceCode();
        string GetBizContent();
        string GetRequestOriginalString();
    }
}
