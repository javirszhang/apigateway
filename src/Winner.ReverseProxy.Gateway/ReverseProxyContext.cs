using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Winner.ReverseProxy.Gateway.Interfaces;

namespace Winner.ReverseProxy.Gateway
{
    /// <summary>
    /// 反向代理上下文
    /// </summary>
    public class ReverseProxyContext
    {
        /// <summary>
        /// 链接查询参数
        /// </summary>
        public IQueryCollection Query { get; set; }
        /// <summary>
        /// body流
        /// </summary>
        public Stream InputStream { get; set; }
        public IParameterModel DataModel { get; set; }
        /// <summary>
        /// 请求内容类型
        /// </summary>
        public string RequestContentType { get; set; }
        /// <summary>
        /// 请求地址
        /// </summary>
        public string RequestUrl { get; set; }
        /// <summary>
        /// 转发地址
        /// </summary>
        public string TransferUrl { get; set; }
        /// <summary>
        /// 代理转发数据
        /// </summary>
        public ProxyForwardValue ForwardValue { get; set; }
        /// <summary>
        /// http请求方法（get/post）
        /// </summary>
        public string HttpMethod { get; set; }
        /// <summary>
        /// 转发http头
        /// </summary>
        public Dictionary<string, string> Headers { get; set; } = new Dictionary<string, string>();
        /// <summary>
        /// 转发cookie
        /// </summary>
        public IRequestCookieCollection Cookies { get; set; }
        public IMerchant Merchant { get; set; }
        public ReverseProxyConfigModel ReverseConfig { get; set; }
    }
}
