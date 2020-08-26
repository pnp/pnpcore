using System;
using System.Text.Json;
using PnP.Core.Services;

namespace PnP.Core.Model
{
    /// <summary>
    /// Base class for all complex types for use complex properties of model classes
    /// </summary>
    /// <typeparam name="TModel">Model class</typeparam>
    internal class BaseComplexType<TModel> : TransientObject, IDataModelMappingHandler
    {
        /// <summary>
        /// Handler that will fire when a property mapping does cannot be done automatically
        /// </summary>
        public Func<FromJson, object> MappingHandler { get; set; } = null;

        /// <summary>
        /// Handler that will fire after the full json to model operation was done
        /// </summary>
        public Action<string> PostMappingHandler { get; set; } = null;

        /// <summary>
        /// Translates model into a set of classes that are used to drive CRUD operations
        /// </summary>
        /// <returns>Entity model class describing this model instance</returns>
        internal EntityInfo GetClassInfo()
        {
            // Get information about the fields to work with and how to handle CRUD operations
            return EntityManager.Instance.GetStaticClassInfo(this.GetType());
        }

        internal static T ToEnum<T>(JsonElement jsonElement)
        {
            if (jsonElement.ValueKind == JsonValueKind.Number && jsonElement.TryGetInt64(out long enumNumericValue))
            {
                if (Enum.TryParse(typeof(T), enumNumericValue.ToString(), out object enumValue))
                {
                    return (T)enumValue;
                }
            }
            else if (jsonElement.ValueKind == JsonValueKind.String && !string.IsNullOrEmpty(jsonElement.GetString()))
            {
                if (Enum.TryParse(typeof(T), jsonElement.GetString(), true, out object enumValue))
                {
                    return (T)enumValue;
                }
            }
            return default;
        }
    }
}
