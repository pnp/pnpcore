using System;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace PnP.Core.Model
{
    /// <summary>
    /// Data model extension methods for public use
    /// </summary>
    internal static class DataModelExtensions
    {
        /// <summary>
        /// Ensures the basic properties (mainly IDs) of the parent of the current domain model object
        /// </summary>
        /// <param name="model">The domain model to which we have to ensure the parent</param>
        internal static async Task EnsureParentObjectAsync(this IDataModelParent model)
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

                var parent = model.Parent;
                if (model.Parent is IManageableCollection)
                {
                    // Parent is a collection, so jump one level up
                    parent = model.Parent.Parent;
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
                    var entityInfo = EntityManager.Instance.GetStaticClassInfo(parent.GetType());
                    if (parent is TransientObject && (parent as TransientObject).HasValue(entityInfo.ActualKeyFieldName))
                    {
                        // Seems as we already have a key...skip the extra query to request it again. This
                        // situation can happen as Requested is only set to true at the end of the JSON parsing
                    }
                    else
                    {
                        // If not, make an explicit request for its basic properties
                        var ensureParentBatch = contextAwareParent.PnPContext.NewBatch();

                        // Define the lambda to retrieve the ID only
                        var expressions = EntityManager.Instance.GetEntityKeyExpressions(parent);

                        // Enqueue the actual request in the dedicated batch
                        await gettableParent.GetBatchAsync(ensureParentBatch, expressions).ConfigureAwait(false);

                        // Make the actual request
                        await contextAwareParent.PnPContext.BatchClient.ExecuteBatch(ensureParentBatch).ConfigureAwait(true);
                    }
                }
            }
        }

        /// <summary>
        /// Sets a property value without marking it as "changed"
        /// </summary>
        /// <typeparam name="TModel">Model type (e.g. ITeamFunSettings)</typeparam>
        /// <typeparam name="T">Value to set</typeparam>
        /// <param name="model">Implementation of the model (e.g. TeamFunSettings)</param>
        /// <param name="expression">Expression listing the property to load</param>
        /// <param name="value">Value to set</param>
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

            (model as TransientObject).SetSystemValue(value, body.Member.Name);
        }        
    }
}
