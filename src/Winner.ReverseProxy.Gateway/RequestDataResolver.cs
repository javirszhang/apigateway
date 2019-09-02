using Javirs.Common.Json;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Winner.Framework.Utils.Model;
using Winner.ReverseProxy.Gateway.Interfaces;

namespace Winner.ReverseProxy.Gateway
{
    public class RequestDataResolver
    {
        public FuncResult<IParameterModel> Resolve(ReverseProxyContext context)
        {
            var result = GetParameterModel(context.HttpMethod, context.InputStream, context.Query);
            if (!result.Success)
            {
                return result;
            }
            if (!result.Content.VerifySignature())
            {
                return FuncResult.FailResult<IParameterModel>("签名验证不正确", 2);
            }
            ContentType contentType = "GET".Equals(context.HttpMethod, StringComparison.OrdinalIgnoreCase) ? ContentType.form_urlencoded : ContentType.json;
            context.DataModel = result.Content;
            context.ForwardValue = new ProxyForwardValue(context.DataModel.GetBizContent(), contentType);
            return FuncResult.SuccessResult(context.DataModel);
        }
        protected virtual FuncResult<IParameterModel> GetParameterModel(string httpMethod, Stream inputStream, IQueryCollection query)
        {
            GatewayParameterModel dataModel;
            if ("POST".Equals(httpMethod, StringComparison.OrdinalIgnoreCase))
            {
                string postData = null;
                using (StreamReader reader = new StreamReader(inputStream))
                {
                    postData = reader.ReadToEnd();
                }
                if (string.IsNullOrEmpty(postData))
                {
                    return FuncResult.FailResult<IParameterModel>("无效报文", 405);
                }
                dataModel = JsonSerializer.Deserializer<GatewayParameterModel>(postData);
            }
            else if ("GET".Equals(httpMethod, StringComparison.OrdinalIgnoreCase))
            {
                dataModel = new GatewayParameterModel();
                dataModel.Data = query["data"];
                dataModel.Sign = query["sign"];
                dataModel.MerchantNo = query["merchantNo"];
                dataModel.Service = query["service"];
                dataModel.Timestamp = Convert.ToInt64(query["timestamp"]);
            }
            else
            {
                return FuncResult.FailResult<IParameterModel>("不支持的请求头", 415);
            }
            return FuncResult.SuccessResult((IParameterModel)dataModel);
        }
    }
}
