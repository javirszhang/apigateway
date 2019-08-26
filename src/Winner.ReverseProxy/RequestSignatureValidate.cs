using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Winner.Framework.Utils.Model;

namespace Winner.ReverseProxy
{
    public class RequestSignatureValidate : IRequestValidate
    {
        public FuncResult Validate(ReverseProxyContext context)
        {            
            
            throw new NotImplementedException();
        }

        public class RequestData {
            public string MerchantNo { get; set; }
            public string Data { get; set; }
            public string Sign { get; set; }
        }

    }
}
