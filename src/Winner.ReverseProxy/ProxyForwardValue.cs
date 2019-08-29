using Javirs.Common.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winner.ReverseProxy
{
    public class ProxyForwardValue
    {
        private Dictionary<string, string> keyValuePairs;
        private string _requestData;
        private ContentType contentType;
        public ProxyForwardValue(string requestData, ContentType contentType)
        {
            keyValuePairs = new Dictionary<string, string>();
            this._requestData = requestData;
            this.contentType = contentType;
        }
        public void Append(string name, string value)
        {
            keyValuePairs.Add(name, value);
        }
        public override string ToString()
        {
            if (string.IsNullOrEmpty(this._requestData) && keyValuePairs.Count <= 0)
            {
                return null;
            }
            if (this.contentType == ContentType.json)
            {
                JsonObject JObject = JsonObject.Parse(this._requestData);
                foreach (var kv in keyValuePairs)
                {
                    JObject.Put(kv.Key, kv.Value);
                }
                return JObject.ToString();
            }
            else
            {
                StringBuilder builder = new StringBuilder(TransformJsonToKeyValuePair(this._requestData));
                keyValuePairs.Aggregate(builder, (b, kv) => b.Append("&").Append(kv.Key).Append("=").Append(kv.Value));
                return builder.ToString().Trim('&');
            }
        }
        private static string TransformJsonToKeyValuePair(string json)
        {
            if (string.IsNullOrEmpty(json) || !json.StartsWith('{'))
            {
                return json;
            }
            StringBuilder stringBuilder = new StringBuilder();
            JsonObject jsonObject = JsonObject.Parse(json);
            foreach (var k in jsonObject.Keys)
            {
                stringBuilder.Append(k).Append("=").Append(jsonObject[k]).Append("&");
            }
            stringBuilder.Remove(stringBuilder.Length - 1, 1);
            return stringBuilder.ToString();
        }
        public byte[] ToArray()
        {
            string str = this.ToString();
            if (string.IsNullOrEmpty(str))
            {
                return null;
            }
            return Encoding.UTF8.GetBytes(str);
        }
    }
}
