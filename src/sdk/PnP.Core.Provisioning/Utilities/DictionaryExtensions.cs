using System.Collections.Generic;

namespace PnP.Core.Provisioning.Utilities
{
    /// <summary>
    /// Extension type for Dictionaries
    /// </summary>
    public static class DictionaryExtensions
    {
        public static void AddRange<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, IDictionary<TKey, TValue> range)
        {
            foreach (var item in range)
            {
                dictionary.Add(item.Key, item.Value);
            }
        }
    }
}
