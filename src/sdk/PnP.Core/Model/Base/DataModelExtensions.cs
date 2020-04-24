using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace PnP.Core.Model
{
    /// <summary>
    /// Data model extension methods for public use
    /// </summary>
    public static class DataModelExtensions
    {
        /// <summary>
        /// Checks if the requested properties are loaded for the given model, if not they're loaded via a GetAsync call
        /// </summary>
        /// <typeparam name="TModel">Model type (e.g. IWeb)</typeparam>
        /// <param name="model">Implementation of the model (e.g. Web)</param>
        /// <param name="expressions">Expressions listing the properties to load</param>
        /// <returns></returns>
        public static async Task EnsurePropertiesAsync<TModel>(this IDataModel<TModel> model, params Expression<Func<TModel, object>>[] expressions)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            var dirty = false;
            List<Expression<Func<TModel, object>>> expressionsToLoad = new List<Expression<Func<TModel, object>>>();
            foreach (Expression<Func<TModel, object>> expression in expressions)
            {
                if (expression.Body.NodeType == ExpressionType.Call && expression.Body is MethodCallExpression)
                {
                    // Future use? (includes)
                }
                else if (!model.IsPropertyAvailable(expression))
                {
                    // Property was not available, add it the expressions to load
                    expressionsToLoad.Add(expression);
                    dirty = true;
                }
            }

            if (dirty)
            {
                await (model as BaseDataModel<TModel>).GetAsync(default, expressionsToLoad.ToArray()).ConfigureAwait(false);
            }            
        }

        /// <summary>
        /// Checks if a property is loaded or not
        /// </summary>
        /// <typeparam name="TModel">Model type (e.g. IWeb)</typeparam>
        /// <param name="model">Implementation of the model (e.g. Web)</param>
        /// <param name="expression">Expression listing the property to load</param>
        /// <returns></returns>
        public static bool IsPropertyAvailable<TModel>(this IDataModel<TModel> model, Expression<Func<TModel, object>> expression)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            if (expression == null)
            {
                throw new ArgumentNullException(nameof(expression));
            }

            var body = expression.Body as MemberExpression ?? ((UnaryExpression)expression.Body).Operand as MemberExpression;

            return model.HasValue(body.Member.Name);
        }

    }
}
