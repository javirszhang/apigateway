using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Collections;

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
            //this.context.TransferUrl = //this.context.RequestUrl;
            Uri local = new Uri(context.RequestUrl);
            string host = string.Concat(local.Host, ":", local.Port);
            var hitmap = context.ReverseConfig.mapping.Find(m =>
            {
                string up = m.upstream;
                if (!m.upstream.Contains(':'))
                {
                    up += ":80";
                }
                return up.Equals(host, StringComparison.OrdinalIgnoreCase);
            });
            if(hitmap == null)
            {
                return false;
            }
            var rdn = new Random();
            var i = rdn.Next(0, hitmap.downstream.Length-1);
            var down = hitmap.downstream[i];
            context.TransferUrl = string.Concat(down.schema, "://", down.host, local.PathAndQuery);
            return true;
        }
    }
    /// <summary>
    /// 反向代理配置model
    /// </summary>
    public class ReverseProxyConfigModel
    {
        public int timeout { get; set; }
        public string[] excludePage { get; set; }
        public List<Mapping> mapping { get; set; }
    }
    public enum MappingType
    {
        FULL_URI = 1,
        SERVICE_CODE = 2
    }

    public class Mapping
    {
        public string upstream { get; set; }
        public Downstream[] downstream { get; set; }
    }

    public class Downstream
    {
        public string host { get; set; }
        public string schema { get; set; }
        public int weight { get; set; }
    }

}
