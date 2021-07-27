using System;
using System.Collections.Generic;
using System.Text;

namespace PnP.Core.Transformation.Extensions
{
    /// <summary>
    /// Extension class to provide additional functionalities on top of Dictionary<TKey, TValue></TKey>
    /// </summary>
    public static class DictionaryExtensions
    {
        public static Dictionary<TKey, TValue> Merge<TKey, TValue>(this Dictionary<TKey, TValue> source, Dictionary<TKey, TValue> merger)
        {
            var result = new Dictionary<TKey, TValue>();

            foreach (var item in source)
            {
                result.Add(item.Key, item.Value);
            }

            if (merger != null)
            {
                foreach (var item in merger)
                {
                    if (!result.ContainsKey(item.Key))
                    {
                        result.Add(item.Key, item.Value);
                    }
                }
            }

            return result;
        }
    }
}
