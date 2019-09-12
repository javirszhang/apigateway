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
    public abstract class RequestDataResolver
    {
        public FuncResult<IParameterModel> Resolve(ReverseProxyContext context)
        {
            var result = GetParameterModel(context.HttpMethod, context.InputStream, context.Query);
            if (!result.Success)
            {
                return result;
            }
            var signV = result.Content.VerifySignature();
            if (!signV.Success)
            {
                return FuncResult.FailResult<IParameterModel>(signV.Message, signV.StatusCode);
            }
            ContentType contentType = "GET".Equals(context.HttpMethod, StringComparison.OrdinalIgnoreCase) ? ContentType.form_urlencoded : ContentType.json;
            context.DataModel = result.Content;
            context.ForwardValue = new ProxyForwardValue(context.DataModel.GetBizContent(), contentType);
            return FuncResult.SuccessResult(context.DataModel);
        }
        protected abstract FuncResult<IParameterModel> GetParameterModel(string httpMethod, Stream inputStream, IQueryCollection query);
    }
}
