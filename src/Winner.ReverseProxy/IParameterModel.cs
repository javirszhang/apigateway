using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Winner.ReverseProxy
{
    public interface IParameterModel
    {
        string GetToken();
        IMerchant GetMerchant();
        string GetRequestSignature();
        bool VerifySignature();
        string GetApiVersion();
        string GetServiceCode();
    }
}
