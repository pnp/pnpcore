using System.Collections.Generic;
using System.Dynamic;

namespace PnP.Core
{
    /// <summary>
    /// Extension type for the ExpandObject type
    /// </summary>
    internal static class ExpandoObjectExtensions
    {
        internal static void SetProperty(this ExpandoObject expando, string propertyName, object propertyValue)
        {
            // We use the IDictionary interface implemented by the ExpandoObject
            var expandoDictionary = expando as IDictionary<string, object>;
            if (expandoDictionary.ContainsKey(propertyName))
                expandoDictionary[propertyName] = propertyValue;
            else
                expandoDictionary.Add(propertyName, propertyValue);
        }

        internal static TProperty GetProperty<TProperty>(this ExpandoObject expando, string propertyName)
        {
            // We use the IDictionary interface implemented by the ExpandoObject
            var expandoDictionary = expando as IDictionary<string, object>;
            if (expandoDictionary.ContainsKey(propertyName))
                return (TProperty)expandoDictionary[propertyName];
            else
                return default(TProperty);
        }

        /// <summary>
        /// Merge several expando objects together
        /// The same properties are overridden with the value of the last specified expando
        /// </summary>
        /// <param name="expando">The initial expando</param>
        /// <param name="others">All the expando to merge</param>
        /// <returns>A new expando instance resulting from the merge of all specified expandos</returns>
        internal static ExpandoObject MergeWith(this ExpandoObject expando, params ExpandoObject[] others)
        {
            var newExpando = new ExpandoObject();
            var newExpandoDictionary = newExpando as IDictionary<string, object>;

            List<ExpandoObject> allExpandos = new List<ExpandoObject>() { expando };
            allExpandos.AddRange(others);

            foreach (var currentExpando in allExpandos)
            {
                var currentExpandoDictionary = currentExpando as IDictionary<string, object>;
                foreach (var currentProp in currentExpandoDictionary)
                {
                    if (newExpandoDictionary.ContainsKey(currentProp.Key))
                        newExpandoDictionary[currentProp.Key] = currentProp.Value;
                    else
                        newExpandoDictionary.Add(currentProp.Key, currentProp.Value);
                }
            }

            return newExpando;
        }
    }
}
