using System.Collections.Generic;
using System.Dynamic;

namespace PnP.Core.Model
{
    /// <summary>
    /// Extension type for the ExpandObject type
    /// </summary>
    internal static class ExpandoObjectExtensions
    {
        public static void SetProperty(this ExpandoObject expando, string propertyName, object propertyValue)
        {
            // We use the IDictionary interface implemented by the ExpandoObject
            var expandoDictionary = expando as IDictionary<string, object>;
            if (expandoDictionary.ContainsKey(propertyName))
                expandoDictionary[propertyName] = propertyValue;
            else
                expandoDictionary.Add(propertyName, propertyValue);
        }

        public static TProperty GetProperty<TProperty>(this ExpandoObject expando, string propertyName)
        {
            // We use the IDictionary interface implemented by the ExpandoObject
            var expandoDictionary = expando as IDictionary<string, object>;
            if (expandoDictionary.ContainsKey(propertyName))
                return (TProperty)expandoDictionary[propertyName];
            else
                return default(TProperty);
        }
    }
}
