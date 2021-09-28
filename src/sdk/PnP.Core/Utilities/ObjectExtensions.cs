using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Reflection;

namespace PnP.Core
{
    internal static class ObjectExtensions
    {
        /// <summary>
        /// Retrieves the value of a public, instance property 
        /// </summary>
        /// <param name="source">The source object</param>
        /// <param name="propertyName">The property name, case insensitive</param>
        /// <returns>The property value, if any</returns>
        internal static object GetPublicInstancePropertyValue(this object source, string propertyName)
        {
            return (source?.GetType()?.GetProperty(propertyName,
                    BindingFlags.Instance |
                    BindingFlags.Public |
                    BindingFlags.IgnoreCase)?
                .GetValue(source));
        }

        private static Dictionary<string, object> AsKeyValues(this object obj,
                    BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase,
                    string[] ignoreProperties = null,
                    bool ignoreNullValues = false)
        {
            Dictionary<string, object> result = new Dictionary<string, object>();

            if (obj == null)
            {
                return result;
            }

            PropertyInfo[] propsInfo = obj.GetType().GetProperties(bindingFlags);

            for (int i = 0; i < propsInfo.Length; i++)
            {
                if (ignoreProperties != null)
                {
                    if (Array.IndexOf(ignoreProperties, propsInfo[i].Name) == -1)
                    {
                        AssignValue(obj, ignoreNullValues, propsInfo, result, i);
                    }
                }
                else
                {
                    AssignValue(obj, ignoreNullValues, propsInfo, result, i);
                }
            }

            return result;

            static void AssignValue(object obj, bool ignoreNullValues, PropertyInfo[] propsInfo, Dictionary<string, object> result, int i)
            {
                if (ignoreNullValues)
                {
                    if (propsInfo[i].GetValue(obj) != null)
                    {
                        result.Add(propsInfo[i].Name, propsInfo[i].GetValue(obj));
                    }
                }
                else
                {
                    result.Add(propsInfo[i].Name, propsInfo[i].GetValue(obj));
                }
            }
        }

        internal static ExpandoObject AsExpando(this object obj,
                    BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase,
                    string[] ignoreProperties = null,
                    bool ignoreNullValues = false)
        {
            var dict = AsKeyValues(obj, bindingFlags, ignoreProperties, ignoreNullValues);
            var expando = new ExpandoObject();

            var expandoDictionary = expando as IDictionary<string, object>;
            foreach (var kvp in dict)
            {
                if (expandoDictionary.ContainsKey(kvp.Key))
                {
                    expandoDictionary[kvp.Key] = kvp.Value;
                }
                else
                {
                    expandoDictionary.Add(kvp.Key, kvp.Value);
                }
            }

            return expando;
        }

        #region Old implementation replaced by above for performance. See PnP.Core.Perf for more context

        //private static Dictionary<string, object> AsKeyValues(this object obj,
        //            BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase,
        //            string[] ignoreProperties = null,
        //            bool ignoreNullValues = false)
        //{
        //    if (obj == null)
        //        return new Dictionary<string, object>();

        //    PropertyInfo[] propsInfo = obj.GetType().GetProperties(bindingFlags);

        //    var qActualPropsQuery = from pi in propsInfo
        //                            select pi;

        //    if (ignoreProperties != null)
        //    {
        //        qActualPropsQuery = from pi in qActualPropsQuery
        //                            where !ignoreProperties.Contains(pi.Name)
        //                            select pi;
        //    }

        //    var qActualPropsKVPQuery = ignoreNullValues
        //        ? from pi in qActualPropsQuery
        //          let key = pi.Name
        //          let value = pi.GetValue(obj)
        //          where value != null
        //          select new { key, value }
        //        : from pi in qActualPropsQuery
        //          let key = pi.Name
        //          let value = pi.GetValue(obj)
        //          select new { key, value };

        //    return qActualPropsKVPQuery.ToDictionary(k => k.key, v => v.value);
        //}

        //internal static ExpandoObject AsExpando(this object obj,
        //            BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase,
        //            string[] ignoreProperties = null,
        //            bool ignoreNullValues = false)
        //{
        //    var dict = AsKeyValues(obj, bindingFlags, ignoreProperties, ignoreNullValues);
        //    var expando = new ExpandoObject();
        //    foreach (var kvp in dict)
        //    {
        //        expando.SetProperty(kvp.Key, kvp.Value);
        //    }
        //    return expando;
        //}

        #endregion
    }
}
