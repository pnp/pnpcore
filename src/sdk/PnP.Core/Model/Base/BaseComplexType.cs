using System;
using System.Linq.Expressions;
using System.Text.Json;
using PnP.Core.Services;

namespace PnP.Core.Model
{
    /// <summary>
    /// Base class for all complex types for use complex properties of model classes
    /// </summary>
    /// <typeparam name="TModel">Model class</typeparam>
    internal class BaseComplexType<TModel> : TransientObject, IDataModelMappingHandler, IRequestable
    {
        /// <summary>
        /// Handler that will fire when a property mapping does cannot be done automatically
        /// </summary>
        public Func<FromJson, object> MappingHandler { get; set; }

        /// <summary>
        /// Handler that will fire after the full json to model operation was done
        /// </summary>
        public Action<string> PostMappingHandler { get; set; }

        /// <summary>
        /// Indicates whether this model was fetched from the server
        /// </summary>
        [SystemProperty]
        public bool Requested { get; set; } = false;

        /// <summary>
        /// Translates model into a set of classes that are used to drive CRUD operations
        /// </summary>
        /// <returns>Entity model class describing this model instance</returns>
        internal EntityInfo GetClassInfo()
        {
            // Get information about the fields to work with and how to handle CRUD operations
            return EntityManager.Instance.GetStaticClassInfo(this.GetType());
        }

        internal static T ToEnum<T>(JsonElement jsonElement) where T : struct
        {
            if (jsonElement.ValueKind == JsonValueKind.Number && jsonElement.TryGetInt64(out long enumNumericValue))
            {
                if (Enum.TryParse(enumNumericValue.ToString(), out T enumValue))
                {
                    return enumValue;
                }
            }
            else if (jsonElement.ValueKind == JsonValueKind.String && !string.IsNullOrEmpty(jsonElement.GetString()))
            {
                if (Enum.TryParse(jsonElement.GetString(), true, out T enumValue))
                {
                    return enumValue;
                }
            }
            return default;
        }

        public bool IsPropertyAvailable(Expression<Func<TModel, object>> expression)
        {
            if (expression == null)
            {
                throw new ArgumentNullException(nameof(expression));
            }

            var body = expression.Body as MemberExpression ?? ((UnaryExpression)expression.Body).Operand as MemberExpression;

            return HasValue(body.Member.Name);
        }
    }
}
