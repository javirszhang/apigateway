using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Winner.ReverseProxy.Gateway.Interfaces
{
    public interface IGatawayResult
    {
        Task ExecuteAsync(HttpContext context);
    }
}
