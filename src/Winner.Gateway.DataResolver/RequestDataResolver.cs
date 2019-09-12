using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Javirs.Common.Json;
using Microsoft.AspNetCore.Http;
using Winner.Framework.Utils.Model;
using Winner.ReverseProxy.Gateway.Interfaces;
using System.Linq;

namespace Winner.Gateway.DataResolver
{
    public class RequestDataResolver : Winner.ReverseProxy.Gateway.RequestDataResolver
    {
        protected override FuncResult<IParameterModel> GetParameterModel(string httpMethod, Stream inputStream, IQueryCollection query)
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
                dataModel.RequestOriginalString = postData;
            }
            else if ("GET".Equals(httpMethod, StringComparison.OrdinalIgnoreCase))
            {
                dataModel = new GatewayParameterModel();
                dataModel.Data = query["data"];
                dataModel.Sign = query["sign"];
                dataModel.MerchantNo = query["merchantNo"];
                dataModel.Service = query["service"];
                dataModel.Timestamp = Convert.ToInt64(query["timestamp"]);
                StringBuilder sb = new StringBuilder();
                query.Aggregate(sb, (b, kv) => b.Append(kv.Key).Append("=").Append(kv.Value).Append("&"));
                if (sb.Length > 1)
                {
                    sb.Remove(sb.Length - 1, 1);
                }
                dataModel.RequestOriginalString = sb.ToString();
            }
            else
            {
                return FuncResult.FailResult<IParameterModel>("不支持的请求头", 415);
            }
            return FuncResult.SuccessResult((IParameterModel)dataModel);
        }
    }
}
