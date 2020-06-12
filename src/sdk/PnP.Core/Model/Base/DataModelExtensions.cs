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
        /// <returns>True if property was loaded, false otherwise</returns>
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

        /// <summary>
        /// Checks if a property is loaded or not on a complex type
        /// </summary>
        /// <typeparam name="TModel">Model type (e.g. ITeamFunSettings)</typeparam>
        /// <param name="model">Implementation of the model (e.g. TeamFunSettings)</param>
        /// <param name="expression">Expression listing the property to load</param>
        /// <returns>True if property was loaded, false otherwise</returns>
        public static bool IsPropertyAvailable<TModel>(this TModel model, Expression<Func<TModel, object>> expression)
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

            return (model as TransientObject).HasValue(body.Member.Name);
        }


        /// <summary>
        /// Ensures the basic properties (mainly IDs) of the parent of the current domain model object
        /// </summary>
        /// <param name="model">The domain model to which we have to ensure the parent</param>
        public static async Task EnsureParentObjectAsync(this IDataModelParent model)
        {
            if (model is null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            if (model.Parent != null)
            {
                // there's either a collection object inbetween (e.g. ListItem --> ListItemCollection --> List), so take the parent of the parent
                // or
                // the parent is model class itself (e.g. Web --> Site.RootWeb)

                var parent = (model as IDataModelParent).Parent;
                if (model.Parent is IManageableCollection)
                {
                    // Parent is a collection, so jump one level up
                    parent = (model as IDataModelParent).Parent.Parent;
                }

                // Let's try to get the parent object as an IRequestable and IDataModelGet instance
                var requestableParent = (IRequestable)parent;
                var gettableParent = (IDataModelGet)parent;
                var contextAwareParent = (IDataModelWithContext)parent;
                // and if successfull, see if it has been already requested
                if (requestableParent != null &&
                    gettableParent != null &&
                    !requestableParent.Requested)
                {
                    // If not, make an explicit request for its basic properties
                    var ensureParentBatch = contextAwareParent.PnPContext.NewBatch();

                    // Define the lambda to retrieve the ID only
                    var expressions = EntityManager.Instance.GetEntityKeyExpressions(parent);

                    // Enqueue the actual request in the dedicated batch
                    gettableParent.Get(ensureParentBatch, expressions);

                    // Make the actual request
                    await contextAwareParent.PnPContext.BatchClient.ExecuteBatch(ensureParentBatch).ConfigureAwait(true);
                }
            }
        }

        /// <summary>
        /// Checks if a property is loaded or not on a complex type
        /// </summary>
        /// <typeparam name="TModel">Model type (e.g. ITeamFunSettings)</typeparam>
        /// <param name="model">Implementation of the model (e.g. TeamFunSettings)</param>
        /// <param name="expression">Expression listing the property to load</param>
        /// <returns>True if property was loaded, false otherwise</returns>
        internal static void SetSystemProperty<TModel, T>(this TModel model, Expression<Func<TModel, object>> expression, T value)
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

            (model as TransientObject).SetSystemValue<T>(value, body.Member.Name);
        }

    }
}
