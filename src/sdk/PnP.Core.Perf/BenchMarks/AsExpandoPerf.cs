using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace PnP.Core.Perf.BenchMarks
{
    public static class AsExpandoPerf
    {
        private static Dictionary<string, object> NewAsKeyValues(this object obj,
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

        internal static void NewSetProperty(this ExpandoObject expando, string propertyName, object propertyValue)
        {
            // We use the IDictionary interface implemented by the ExpandoObject
            var expandoDictionary = expando as IDictionary<string, object>;
            if (expandoDictionary.ContainsKey(propertyName))
                expandoDictionary[propertyName] = propertyValue;
            else
                expandoDictionary.Add(propertyName, propertyValue);
        }

        internal static ExpandoObject NewAsExpando(this object obj,
                    BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase,
                    string[] ignoreProperties = null,
                    bool ignoreNullValues = false)
        {
            var dict = NewAsKeyValues(obj, bindingFlags, ignoreProperties, ignoreNullValues);
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


        private static Dictionary<string, object> BaseAsKeyValues(this object obj,
                    BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase,
                    string[] ignoreProperties = null,
                    bool ignoreNullValues = false)
        {
            if (obj == null)
                return new Dictionary<string, object>();

            PropertyInfo[] propsInfo = obj.GetType().GetProperties(bindingFlags);

            var qActualPropsQuery = from pi in propsInfo
                                    select pi;

            if (ignoreProperties != null)
            {
                qActualPropsQuery = from pi in qActualPropsQuery
                                    where !ignoreProperties.Contains(pi.Name)
                                    select pi;
            }

            var qActualPropsKVPQuery = ignoreNullValues
                ? from pi in qActualPropsQuery
                  let key = pi.Name
                  let value = pi.GetValue(obj)
                  where value != null
                  select new { key, value }
                : from pi in qActualPropsQuery
                  let key = pi.Name
                  let value = pi.GetValue(obj)
                  select new { key, value };

            return qActualPropsKVPQuery.ToDictionary(k => k.key, v => v.value);
        }

        internal static ExpandoObject BaseAsExpando(this object obj,
                    BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase,
                    string[] ignoreProperties = null,
                    bool ignoreNullValues = false)
        {
            var dict = BaseAsKeyValues(obj, bindingFlags, ignoreProperties, ignoreNullValues);
            var expando = new ExpandoObject();
            foreach (var kvp in dict)
            {
                expando.BaseSetProperty(kvp.Key, kvp.Value);
            }
            return expando;
        }

        internal static void BaseSetProperty(this ExpandoObject expando, string propertyName, object propertyValue)
        {
            // We use the IDictionary interface implemented by the ExpandoObject
            var expandoDictionary = expando as IDictionary<string, object>;
            if (expandoDictionary.ContainsKey(propertyName))
                expandoDictionary[propertyName] = propertyValue;
            else
                expandoDictionary.Add(propertyName, propertyValue);
        }
    }
}
