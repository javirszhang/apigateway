using Javirs.Common.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Winner.Framework.Utils.Model;

namespace Winner.ReverseProxy
{
    public class RequestDataResolver
    {
        public FuncResult<IParameterModel> Resolve(ReverseProxyContext context)
        {
            ContentType cttType;
            if ("POST".Equals(context.HttpMethod, StringComparison.OrdinalIgnoreCase))
            {
                string postData = null;
                using (StreamReader reader = new StreamReader(context.InputStream))
                {
                    postData = reader.ReadToEnd();
                }
                if (string.IsNullOrEmpty(postData))
                {
                    return FuncResult.FailResult<IParameterModel>("无效报文", 405);
                }
                context.DataModel = JsonSerializer.Deserializer<GatewayParameterModel>(postData);
                cttType = ContentType.json;
            }
            else if ("GET".Equals(context.HttpMethod, StringComparison.OrdinalIgnoreCase))
            {
                context.DataModel = new GatewayParameterModel();
                cttType = ContentType.form_urlencoded;
                context.DataModel.Data = context.Query["data"];
                context.DataModel.Sign = context.Query["sign"];
                context.DataModel.MerchantNo = context.Query["merchantNo"];
                context.DataModel.Service = context.Query["service"];
                context.DataModel.Timestamp = Convert.ToInt64(context.Query["timestamp"]);
            }
            else
            {
                return FuncResult.FailResult<IParameterModel>("不支持的请求头", 415);
            }
            context.DataModel.Data = context.DataModel.GetMerchant().Decrypt(context.DataModel.Data);
            
            context.ForwardValue = new ProxyForwardValue(context.DataModel.Data, cttType);
            if (!context.DataModel.VerifySignature())
            {
                return FuncResult.FailResult<IParameterModel>("签名验证不正确", 2);
            }
            return FuncResult.SuccessResult((IParameterModel)context.DataModel);
        }
    }
}
