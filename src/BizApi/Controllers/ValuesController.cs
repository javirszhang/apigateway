using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BizApi.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        // GET api/values
        [HttpGet]
        public ActionResult<IEnumerable<string>> Get()
        {
            this.HttpContext.Session.SetString("value", "session:" + Request.QueryString.Value);
            return new string[] { "value1", "value2", Request.QueryString.Value, Request.Host.Value };
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public ActionResult<string> Get(int id)
        {
            return "session value is :" + this.HttpContext.Session.GetString("value");
        }

        // POST api/values
        [HttpPost()]
        public object Post(PostModel model)
        {
            object data = new { resCode = "0000", resMsg = "ok", value = $"post {model.UserCode}=={model.UserId}" };
            return Ok(data);
        }
        public class PostModel
        {
            public string UserCode { get; set; }
            public int UserId { get; set; }
        }
        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }
        public object GetUserInfo()
        {
            return new { userId = 1, userName = "Jason", userCode = "12284682225" };
        }
        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
