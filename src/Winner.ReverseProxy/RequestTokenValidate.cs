using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Winner.Framework.Utils.Model;

namespace Winner.ReverseProxy
{
    public class RequestTokenValidate : IRequestValidate
    {
        public FuncResult Validate(ReverseProxyContext context)
        {
            context.ForwardValue.Append("userCode", "12284682225");
            context.ForwardValue.Append("userId", "100201");
            return FuncResult.SuccessResult();
        }
    }
}
