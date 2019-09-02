using System;
using System.Collections.Generic;
using System.Text;
using Winner.Framework.Utils.Model;

namespace Winner.ReverseProxy.Gateway.Interfaces
{
    public interface IRequestValidate
    {
        FuncResult Validate(ReverseProxyContext context);
    }
}
