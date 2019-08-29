using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Winner.ReverseProxy
{
    /// <summary>
    /// 反向代理配置model
    /// </summary>
    public class ReverseProxyConfigModel
    {
        public int timeout { get; set; }
        public string[] excludePages { get; set; }
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
        public int weight { get; set; }
    }
}
