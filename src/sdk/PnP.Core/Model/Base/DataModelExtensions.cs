using PnP.Core.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
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
            List<Expression<Func<TModel, object>>> expressionsToLoad = VerifyProperties(model, expressions, ref dirty);

            if (dirty)
            {
                await (model as BaseDataModel<TModel>).GetAsync(default, expressionsToLoad.ToArray()).ConfigureAwait(false);
            }
        }

        private static List<Expression<Func<TModel, object>>> VerifyProperties<TModel>(IDataModel<TModel> model, Expression<Func<TModel, object>>[] expressions, ref bool dirty)
        {
            List<Expression<Func<TModel, object>>> expressionsToLoad = new List<Expression<Func<TModel, object>>>();
            foreach (Expression<Func<TModel, object>> expression in expressions)
            {
                if (expression.Body.NodeType == ExpressionType.Call && expression.Body is MethodCallExpression)
                {
                    // Future use? (includes)
                    var body = (MethodCallExpression)expression.Body;
                    if (body.Method.IsGenericMethod && body.Method.Name == "Include")
                    {
                        if (body.Arguments.Count != 2)
                        {
                            throw new Exception("Invalid arguments number");
                        }

                        // Parse the expressions and get the relevant entity information
                        var entityInfo = EntityManager.Instance.GetClassInfo<TModel>(model.GetType(), (model as BaseDataModel<TModel>), expressions);

                        string fieldToLoad = (body.Arguments[0] as MemberExpression).Member.Name;

                        var collectionToCheck = entityInfo.Fields.FirstOrDefault(p => p.Name == fieldToLoad);
                        if (collectionToCheck != null)
                        {
                            var collection = model.GetPublicInstancePropertyValue(fieldToLoad);
                            if (collection is IRequestableCollection)
                            {
                                if (!(collection as IRequestableCollection).Requested)
                                {
                                    // Collection was not requested at all, so let's load it again
                                    expressionsToLoad.Add(expression);
                                    dirty = true;
                                }
                                else
                                {
                                    if (collectionToCheck.ExpandFieldInfo != null && (collection as IRequestableCollection).Length > 0)
                                    {
                                        // Collection was requested and there's at least one item to check, let's see if we can figure out if all needed properties were loaded as well
                                        if (!WereFieldsRequested(collectionToCheck, collectionToCheck.ExpandFieldInfo, collection as IRequestableCollection))
                                        {
                                            expressionsToLoad.Add(expression);
                                            dirty = true;
                                        }
                                    }
                                    else
                                    {
                                        expressionsToLoad.Add(expression);
                                        dirty = true;
                                    }
                                }
                            }
                        }
                        else
                        {
                            expressionsToLoad.Add(expression);
                            dirty = true;
                        }
                    }
                    else
                    {
                        throw new ClientException(ErrorType.PropertyNotLoaded, "Only the 'Include' method is supported");
                    }
                }
                else if (!model.IsPropertyAvailable(expression))
                {
                    // Property was not available, add it the expressions to load
                    expressionsToLoad.Add(expression);
                    dirty = true;
                }
            }

            return expressionsToLoad;
        }

        private static bool WereFieldsRequested(EntityFieldInfo collectionToCheck, EntityFieldExpandInfo expandFields, IRequestableCollection collection)
        {
            var enumerator = collection.RequestedItems.GetEnumerator();

            enumerator.Reset();
            enumerator.MoveNext();

            // If no item available or collection was not requested then we need to reload
            if (!collection.Requested || enumerator.Current == null)
            {
                return false;
            }

            TransientObject itemToCheck = enumerator.Current as TransientObject;

            foreach (var fieldInExpression in expandFields.Fields)
            {
                if (!fieldInExpression.Fields.Any())
                {
                    if (!itemToCheck.HasValue(fieldInExpression.Name))
                    {
                        return false;
                    }
                }
                else
                {
                    // this is another collection, so perform a recursive check
                    var collectionRecursive = itemToCheck.GetPublicInstancePropertyValue(fieldInExpression.Name) as IRequestableCollection;
                    if (!WereFieldsRequested(collectionToCheck, fieldInExpression, collectionRecursive))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Checks if the needed properties were loaded or not
        /// </summary>
        /// <typeparam name="TModel">Model type (e.g. IWeb)</typeparam>
        /// <param name="model">Implementation of the model (e.g. Web)</param>
        /// <param name="expressions">Expression listing the properties to check</param>
        /// <returns>True if properties were loaded, false otherwise</returns>
        public static bool ArePropertiesAvailable<TModel>(this IDataModel<TModel> model, params Expression<Func<TModel, object>>[] expressions)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            var dirty = false;
            VerifyProperties(model, expressions, ref dirty);

            return !dirty;
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
                    await gettableParent.GetBatchAsync(ensureParentBatch, expressions).ConfigureAwait(false);

                    // Make the actual request
                    await contextAwareParent.PnPContext.BatchClient.ExecuteBatch(ensureParentBatch).ConfigureAwait(true);
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

            (model as TransientObject).SetSystemValue<T>(value, body.Member.Name);
        }


        /// <summary>
        /// Enables using the .LoadProperties lambda expression syntax on a collection
        /// </summary>
        /// <typeparam name="TModel">Collection model</typeparam>
        /// <param name="collection">Collection to apply the .LoadProperties on </param>
        /// <param name="expressions">Expression</param>
        /// <returns>Null...return value is not needed</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "<Pending>")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA1801:Review unused parameters", Justification = "<Pending>")]
        internal static IQueryable<TModel> LoadProperties<TModel>(this IDataModelCollection<TModel> collection, params Expression<Func<TModel, object>>[] expressions)
        {
            return null;
        }

    }
}
