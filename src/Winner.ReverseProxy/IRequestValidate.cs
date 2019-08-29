using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Winner.Framework.Utils.Model;

namespace Winner.ReverseProxy
{
    public interface IRequestValidate
    {
        FuncResult Validate(ReverseProxyContext context);
    }
}
