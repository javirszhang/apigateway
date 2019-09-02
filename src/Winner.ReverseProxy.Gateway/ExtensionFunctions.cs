
using Microsoft.Extensions.Configuration;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Winner.ReverseProxy.Gateway;
using Winner.ReverseProxy.Gateway.Interfaces;

namespace Winner.ReverseProxy.Gateway
{
    public static class ExtensionFunctions
    {
        public static int Count(this IEnumerable array)
        {
            int i = 0;
            foreach (object item in array)
            {
                i++;
            }
            return i;
        }
        public static bool Contains(this IEnumerable<string> array, string item, StringComparison comparison = StringComparison.CurrentCulture)
        {
            foreach (string s in array)
            {
                if (s.Equals(item, comparison))
                {
                    return true;
                }
            }
            return false;
        }
        public static bool Remove<TKey, TValue>(this IDictionary<TKey, TValue> keyValues, Predicate<KeyValuePair<TKey, TValue>> match)
        {
            List<TKey> keys = new List<TKey>();
            foreach (KeyValuePair<TKey, TValue> keyValuePair in keyValues)
            {
                if (match(keyValuePair))
                {
                    keys.Add(keyValuePair.Key);
                }
            }
            foreach (TKey key in keys)
            {
                keyValues.Remove(key);
            }
            return true;
        }
        public static void Aggregate<TBuilder, TKey, TValue>(this IDictionary<TKey, TValue> keyValues, TBuilder builder, Func<TBuilder, KeyValuePair<TKey, TValue>, TBuilder> procedure)
        {
            foreach (var keyValuePair in keyValues)
            {
                procedure(builder, keyValuePair);
            }
        }
    }
}

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ExtensionFunctions
    {
        public static void AddRequestValidation<T>(this IServiceCollection services) where T : class, IRequestValidate
        {
            services.AddScoped<IRequestValidate, T>();
        }

        public static void AddReverseProxyConfig(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<ReverseProxyConfigModel>(configuration.GetSection("ReverseProxy"));
        }
    }
}
