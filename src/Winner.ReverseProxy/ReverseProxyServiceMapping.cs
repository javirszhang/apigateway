using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Collections;
using Winner.Framework.Utils;

namespace Winner.ReverseProxy
{
    /// <summary>
    /// 反向代理服务映射
    /// </summary>
    public class ReverseProxyServiceMapping
    {
        public ReverseProxyServiceMapping()
        {
        }

        public virtual bool Resolve(ReverseProxyContext context)
        {
            Log.Info($"service={context.DataModel?.Service}");
            Mapping hitmap = null;
            string path = null;
            if (!string.IsNullOrEmpty(context.DataModel?.Service))
            {
                
                hitmap = context.ReverseConfig.mapping.Find(m => context.DataModel.Service.Equals(m.upstream, StringComparison.Ordinal));
            }
            else
            {
                Uri local = new Uri(context.RequestUrl);
                path = local.AbsolutePath;
                string host = string.Concat(local.Host, ":", local.Port);
                hitmap = context.ReverseConfig.mapping.Find(m =>
                {
                    string up = m.upstream;
                    if (!m.upstream.Contains(':'))
                    {
                        up += ":80";
                    }
                    return up.Equals(host, StringComparison.OrdinalIgnoreCase);
                });
            }
            if (hitmap == null)
            {
                return false;
            }
            var rdn = new Random();
            var i = rdn.Next(0, hitmap.downstream.Length - 1);
            var down = hitmap.downstream[i];
            context.TransferUrl = string.Concat(down.host, path);
            return true;
        }
    }
}
