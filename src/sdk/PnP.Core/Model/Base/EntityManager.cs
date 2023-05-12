using PnP.Core.Services;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace PnP.Core.Model
{
    /// <summary>
    /// Singleton class that serves as a simple caching of entity information
    /// </summary>
    internal sealed class EntityManager
    {
        private static readonly Lazy<EntityManager> _lazyInstance = new Lazy<EntityManager>(() => new EntityManager(), true);

        private readonly ConcurrentDictionary<Type, EntityInfo> entityCache;

        /// <summary>
        /// Provides the singleton instance of th entity manager
        /// </summary>
        internal static EntityManager Instance
        {
            get
            {
                return _lazyInstance.Value;
            }
        }

        /// <summary>
        /// Private constructor since this is a singleton
        /// </summary>
        private EntityManager()
        {
            entityCache = new ConcurrentDictionary<Type, EntityInfo>();
        }

        /// <summary>
        /// Translates model type into a set of classes that are used to drive CRUD operations
        /// </summary>
        /// <param name="type">The reference model type</param>
        /// <returns>Entity model class describing this model instance</returns>
        internal EntityInfo GetStaticClassInfo(Type type)
        {
            type = GetEntityConcreteType(type);

            // Check if we can deliver this entity from cache
            if (entityCache.TryGetValue(type, out EntityInfo entityInfoFromCache))
            {
                return entityInfoFromCache;
            }
            else
            {
                // Load and process type attributes
                var sharePointTypeAttributes = type.GetCustomAttributes<SharePointTypeAttribute>(false);
                var graphTypeAttributes = type.GetCustomAttributes<GraphTypeAttribute>(false);

                if (sharePointTypeAttributes.Any() || graphTypeAttributes.Any())
                {
                    EntityInfo classInfo = new EntityInfo
                    {
                        UseOverflowField = type.ImplementsInterface(typeof(IExpandoDataModel))
                    };

                    if (sharePointTypeAttributes.Any())
                    {
                        foreach (var sharePointTypeAttribute in sharePointTypeAttributes)
                        {
                            var sharePointTargetToAdd = new EntitySharePointTypeInfo
                            {
                                Type = sharePointTypeAttribute.Type,
                                Target = sharePointTypeAttribute.Target ?? type,
                                Uri = sharePointTypeAttribute.Uri,
                                Get = !string.IsNullOrEmpty(sharePointTypeAttribute.Get) ? sharePointTypeAttribute.Get : sharePointTypeAttribute.Uri,
                                LinqGet = !string.IsNullOrEmpty(sharePointTypeAttribute.LinqGet) ? sharePointTypeAttribute.LinqGet : sharePointTypeAttribute.Uri,
                                OverflowProperty = sharePointTypeAttribute.OverflowProperty,
                                Update = !string.IsNullOrEmpty(sharePointTypeAttribute.Update) ? sharePointTypeAttribute.Update : sharePointTypeAttribute.Uri,
                                Delete = !string.IsNullOrEmpty(sharePointTypeAttribute.Delete) ? sharePointTypeAttribute.Delete : sharePointTypeAttribute.Uri,
                            };

                            classInfo.SharePointTargets.Add(sharePointTargetToAdd);
                        }
                    }

                    if (graphTypeAttributes.Any())
                    {
                        foreach (var graphTypeAttribute in graphTypeAttributes)
                        {
                            var graphTargetToAdd = new EntityGraphTypeInfo
                            {
                                Target = graphTypeAttribute.Target ?? type,
                                Id = !string.IsNullOrEmpty(graphTypeAttribute.Id) ? graphTypeAttribute.Id : "id",
                                Get = !string.IsNullOrEmpty(graphTypeAttribute.Get) ? graphTypeAttribute.Get : graphTypeAttribute.Uri,
                                LinqGet = !string.IsNullOrEmpty(graphTypeAttribute.LinqGet) ? graphTypeAttribute.LinqGet : graphTypeAttribute.Uri,
                                OverflowProperty = graphTypeAttribute.OverflowProperty,
                                Update = !string.IsNullOrEmpty(graphTypeAttribute.Update) ? graphTypeAttribute.Update : graphTypeAttribute.Uri,
                                Delete = !string.IsNullOrEmpty(graphTypeAttribute.Delete) ? graphTypeAttribute.Delete : graphTypeAttribute.Uri,
                                Beta = graphTypeAttribute.Beta,
                            };

                            classInfo.GraphTargets.Add(graphTargetToAdd);
                        }
                    }

                    string keyPropertyName = null;
                    foreach (var property in type.GetProperties())
                    {
                        EntityFieldInfo classField = null;

                        var propertyAttributes = property.GetCustomAttributes();
                        bool skipField = false;
                        foreach (var attribute in propertyAttributes)
                        {
                            switch (attribute)
                            {
                                // Field metadata
                                case SharePointPropertyAttribute sharePointPropertyAttribute:
                                    {
                                        classField = EnsureClassField(type, property, classInfo);
                                        classField.SharePointName = !string.IsNullOrEmpty(sharePointPropertyAttribute.FieldName) ? sharePointPropertyAttribute.FieldName : property.Name;
                                        classField.ExpandableByDefault = sharePointPropertyAttribute.ExpandByDefault;
                                        classField.SharePointUseCustomMapping = sharePointPropertyAttribute.UseCustomMapping;
                                        classField.SharePointJsonPath = sharePointPropertyAttribute.JsonPath;
                                        break;
                                    }
                                case GraphPropertyAttribute graphPropertyAttribute:
                                    {
                                        classField = EnsureClassField(type, property, classInfo);
                                        classField.GraphName = !string.IsNullOrEmpty(graphPropertyAttribute.FieldName) ? graphPropertyAttribute.FieldName : ToCamelCase(property.Name);
                                        classField.GraphExpandable = graphPropertyAttribute.Expandable;
                                        classField.ExpandableByDefault = graphPropertyAttribute.ExpandByDefault;
                                        classField.GraphUseCustomMapping = graphPropertyAttribute.UseCustomMapping;
                                        classField.GraphJsonPath = graphPropertyAttribute.JsonPath;
                                        classField.GraphGet = graphPropertyAttribute.Get;
                                        classField.GraphBeta = graphPropertyAttribute.Beta;
                                        break;
                                    }
                                case KeyPropertyAttribute keyPropertyAttribute:
                                    {
                                        keyPropertyName = keyPropertyAttribute.KeyPropertyName;
                                        skipField = true;
                                        break;
                                    }
                                case SystemPropertyAttribute systemPropertyAttribute:
                                    {
                                        skipField = true;
                                        break;
                                    }
                            }
                        }

                        if (!skipField)
                        {
                            if (classField == null)
                            {
                                classField = EnsureClassField(type, property, classInfo);
                                // Property was not decorated with attributes
                                if (!classInfo.SharePointTargets.Any())
                                {
                                    // This is a Graph only property
                                    classField.GraphName = ToCamelCase(property.Name);
                                }
                                else
                                {
                                    // This is SharePoint/Graph property, we're not setting the GraphName here because in "mixed" objects the Graph properties must be explicitely marked with the GraphProperty attribute
                                    classField.SharePointName = property.Name;
                                }
                            }

                            // Automatically determine "expand" value for SharePoint properties
                            if (!string.IsNullOrEmpty(classField.SharePointName))
                            {

                                if (JsonMappingHelper.IsModelCollection(classField.PropertyInfo.PropertyType) ||
                                    JsonMappingHelper.IsModelType(classField.PropertyInfo.PropertyType))
                                {
                                    classField.SharePointExpandable = true;
                                }
                            }
                        }
                    }

                    // Find the property set as key field and mark it as such
                    if (!string.IsNullOrEmpty(keyPropertyName))
                    {
                        // Store the actual key property name
                        classInfo.ActualKeyFieldName = keyPropertyName;

                        // Process the SharePoint and Graph ID fields
                        var keyProperty = classInfo.Fields.FirstOrDefault(p => p.Name == keyPropertyName);
                        if (keyProperty != null)
                        {
                            if (classInfo.SharePointTargets.Any())
                            {
                                keyProperty.IsSharePointKey = true;
                            }

                            keyProperty.IsGraphKey = true;
                            // If a property is defined as graph key then ensure the GraphName is correctly set
                            if (string.IsNullOrEmpty(keyProperty.GraphName) && classInfo.GraphTargets.Any())
                            {
                                keyProperty.GraphName = ToCamelCase(keyProperty.Name);
                            }
                        }
                    }

                    // Update the field used for overflow, this field was added (as it's part of the ExpandoBaseDataModel base data class),
                    // But since there's no field to set properties on the fieldname property comes from the class mapping attribute
                    if (classInfo.UseOverflowField)
                    {
                        var overflowField = classInfo.Fields.FirstOrDefault(p => p.Name == ExpandoBaseDataModel<IExpandoDataModel>.OverflowFieldName);

                        if (!classInfo.SharePointTargets.Any())
                        {
                            // This is a Graph only property
                            overflowField.GraphName = classInfo.GraphTargets.First().OverflowProperty;
                        }
                        else
                        {
                            // This is SharePoint/Graph property
                            overflowField.SharePointName = classInfo.SharePointTargets.First().OverflowProperty;
                            overflowField.GraphName = classInfo.GraphTargets.FirstOrDefault()?.OverflowProperty;
                        }
                    }

                    // Add to our cache to speed up future retrievals
                    entityCache.TryAdd(type, classInfo);
                    return classInfo;

                }
                else
                {
                    throw new ClientException(ErrorType.ModelMetadataIncorrect,
                        PnPCoreResources.Exception_ModelMetadataIncorrect_MissingClassMapping);
                }
            }
        }

        /// <summary>
        /// Creates a concrete instance of a domain model type based on the reference type
        /// </summary>
        /// <param name="type">The reference model type, can be an interface or a class</param>
        /// <param name="parent">Parent of the domain model object, optional</param>
        /// <param name="context">The PnPContext to attach to the newly created item</param>
        /// <returns>Entity model class describing this model instance</returns>
        internal static object GetEntityConcreteInstance(Type type, IDataModelParent parent = null, PnPContext context = null)
        {
            if (type is null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            // Create the object instance
            type = GetEntityConcreteType(type);
            var result = Activator.CreateInstance(type);

            // Set the parent, if any
            if (parent != null && result is IDataModelParent modelWithParent)
            {
                // Plug the parent into the concrete instance
                modelWithParent.Parent = parent;
            }

            // Set the PnPContext, if any
            if (context != null && result is IDataModelWithContext modelWithContext)
            {
                modelWithContext.PnPContext = context;
            }

            return result;
        }

        internal static object GetEntityCollectionInstance(Type collectionType, PnPContext context, IDataModelParent parent, string propertyName)
        {
            object result;

            if (collectionType.ImplementsInterface(typeof(IQueryable<>)))
            {
                result = Activator.CreateInstance(collectionType, context, parent, propertyName);
            }
            else
            {
                result = Activator.CreateInstance(collectionType);
                if (result is IDataModelParent modelWithParent)
                {
                    modelWithParent.Parent = parent;
                }

                if (result is IDataModelWithContext modelWithContext)
                {
                    modelWithContext.PnPContext = context;
                }
            }

            return result;
        }

        internal static object GetEntityCollectionConcreteInstance(Type type, PnPContext context, IDataModelParent parent, string propertyName)
        {
            if (type is null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            type = GetEntityConcreteType(type);

            object result = GetEntityCollectionInstance(type, context, parent, propertyName);

            return result;
        }

        /// <summary>
        /// Translates model into a set of classes that are used to drive CRUD operations, this takes into account the passed expressions
        /// </summary>
        /// <param name="modelType">The Type of the model object to process</param>
        /// <param name="target">Model instance we're working on</param>
        /// <param name="parent">Model instance's parent object, optional</param>
        /// <param name="expressions">Data load expressions, optional</param>
        /// <returns>Entity model class describing this model instance</returns>
        internal static EntityInfo GetClassInfo<TModel>(Type modelType, BaseDataModel<TModel> target, IDataModelParent parent = null, params Expression<Func<TModel, object>>[] expressions)
        {
            // Get static information about the fields to work with and how to handle CRUD operations
            var staticClassInfo = Instance.GetStaticClassInfo(modelType);

            // Copy static info in dynamic model to allow instance specific updates
            EntityInfo entityInfo = new EntityInfo(staticClassInfo);

            // if the user specified an expression then update our class info to reflect that 
            if (expressions != null && expressions.Any())
            {
                var nonExpandableGraphCollections = entityInfo.GraphNonExpandableCollections;
                bool nonExpandableGraphCollectionSkipped = false;

                List<string> graphFieldsToLoad = new List<string>();
                List<string> sharePointFieldsToLoad = new List<string>();
                foreach (Expression<Func<TModel, object>> expression in expressions)
                {
                    string fieldToLoad = GetFieldToLoad(entityInfo, expression);

                    if (fieldToLoad != null)
                    {
                        // Non expandable collections do not count as regular field as they're loaded via an additional query
                        if (nonExpandableGraphCollections.Any())
                        {
                            if (nonExpandableGraphCollections.FirstOrDefault(p => p.Name.Equals(fieldToLoad, StringComparison.InvariantCultureIgnoreCase)) == null)
                            {
                                graphFieldsToLoad.Add(fieldToLoad);
                            }
                            else
                            {
                                // We're not loading this collection as we're using a separate query, but in case 
                                // this collection was the only requested one (e.g. web.GetAsync(p=>p.Lists)) we still need 
                                // process our field load settings later on
                                nonExpandableGraphCollectionSkipped = true;
                            }
                        }
                        else
                        {
                            graphFieldsToLoad.Add(fieldToLoad);
                        }

                        sharePointFieldsToLoad.Add(fieldToLoad);
                    }
                }


                if (graphFieldsToLoad.Count > 0 || nonExpandableGraphCollectionSkipped)
                {
                    // Indicate that this entity information used an expression, will be used when building get queries
                    entityInfo.GraphFieldsLoadedViaExpression = true;

                    foreach (var field in entityInfo.Fields)
                    {
                        if (!sharePointFieldsToLoad.Contains(field.Name))
                        {
                            field.Load = false;
                        }
                    }
                }

                if (sharePointFieldsToLoad.Count > 0)
                {
                    entityInfo.SharePointFieldsLoadedViaExpression = true;
                }
            }

            // If fields are marked as key field we always include them in the load 
            foreach (var field in entityInfo.Fields.Where(p => p.IsGraphKey || p.IsSharePointKey))
            {
                field.Load = true;
            }

            if (target != null && parent == null)
            {
                // In case a model can be used from different contexts (e.g. ContentType can be used from Web, but also from List)
                // it's required to let the entity know this context so that it can provide the correct information when requested
                parent = (target as IDataModelParent).Parent;
                if (parent is IManageableCollection)
                {
                    // Parent is a collection, so jump one level up
                    parent = (target as IDataModelParent).Parent.Parent;
                }
            }
            else if (target == null && parent != null)
            {
                // Otherwise fallback to the provided parent, if any
                if (parent is IManageableCollection)
                {
                    // Parent is a collection, so jump one level up
                    parent = parent.Parent;
                }
            }

            if (parent != null)
            {
                entityInfo.Target = parent.GetType();
            }

            return entityInfo;
        }

        internal static string GetFieldToLoad<TModel>(EntityInfo entityInfo, Expression<Func<TModel, object>> expression)
        {
            string fieldToLoad = null;

            if (expression.Body is MemberExpression)
            {
                if (((MemberExpression)expression.Body).Expression is MemberExpression)
                {
                    throw new ClientException(ErrorType.Unsupported, PnPCoreResources.Exception_PropertyNotLoaded_NestedProperties);
                }

                fieldToLoad = ((MemberExpression)expression.Body).Member.Name;
            }
            else if (expression.Body is UnaryExpression)
            {
                var a = (expression.Body as UnaryExpression).Operand;
                if (a is MemberExpression)
                {
                    fieldToLoad = (a as MemberExpression).Member.Name;
                }
            }
            else if (expression.Body is MethodCallExpression)
            {
                if ((expression.Body as MethodCallExpression).Method.Name == nameof(DataModelLoadExtensions.QueryProperties))
                {
                    fieldToLoad = ParseQueryProperties(entityInfo, expression, null);
                }
            }

            return fieldToLoad;
        }

        private static string ParseQueryProperties(EntityInfo entityInfo, LambdaExpression expression, EntityFieldExpandInfo entityFieldExpandInfo)
        {
            var resultType = ((expression).Body as MethodCallExpression).Type;

            // If the resulting type is a requestable collection
            if (resultType.Name == typeof(IQueryable<>).Name)
            {
                // We get the result type from the generic argument of the collection
                resultType = resultType.GenericTypeArguments[0];
            }

            var fieldToLoad = ((expression.Body as MethodCallExpression).Arguments[0] as MemberExpression).Member.Name;
            var resultTypeInfo = Instance.GetStaticClassInfo(resultType);

            bool first = false;
            if (entityFieldExpandInfo == null)
            {
                first = true;
                entityFieldExpandInfo = new EntityFieldExpandInfo()
                {
                    Name = fieldToLoad,
                    Type = resultType
                };
            }

            List<string> expandFieldsToLoad = new List<string>();

            foreach (var includeFieldExpression in ((expression.Body as MethodCallExpression).Arguments[1] as NewArrayExpression).Expressions)
            {
                string expandFieldToLoad = null;
                if (includeFieldExpression is UnaryExpression)
                {
                    var fieldExpressionBody = ((includeFieldExpression as UnaryExpression).Operand as LambdaExpression).Body;
                    if (fieldExpressionBody is MemberExpression)
                    {
                        expandFieldToLoad = (fieldExpressionBody as MemberExpression).Member.Name;

                        var expandField = resultTypeInfo.Fields.First(p => p.Name == expandFieldToLoad);

                        entityFieldExpandInfo.Fields.Add(new EntityFieldExpandInfo() { Name = expandFieldToLoad, Expandable = expandField.SharePointExpandable || expandField.GraphExpandable });
                    }
                    else if (fieldExpressionBody is UnaryExpression)
                    {
                        expandFieldToLoad = ((fieldExpressionBody as UnaryExpression).Operand as MemberExpression).Member.Name;

                        var expandField = resultTypeInfo.Fields.First(p => p.Name == expandFieldToLoad);

                        entityFieldExpandInfo.Fields.Add(new EntityFieldExpandInfo() { Name = expandFieldToLoad, Expandable = expandField.SharePointExpandable || expandField.GraphExpandable });
                    }
                    else if (fieldExpressionBody is MethodCallExpression)
                    {
                        if ((fieldExpressionBody as MethodCallExpression).Method.Name == nameof(DataModelLoadExtensions.QueryProperties))
                        {
                            var fld = ((fieldExpressionBody as MethodCallExpression).Arguments[0] as MemberExpression).Member.Name;

                            Type publicTypeRecursive = null;
                            if ((fieldExpressionBody as MethodCallExpression).Type.GetGenericArguments().Length > 0)
                            {
                                // Expandable property (e.g. List.RootFolder)
                                publicTypeRecursive = (fieldExpressionBody as MethodCallExpression).Type.GenericTypeArguments[0];
                            }
                            else
                            {
                                // Expandable collection (e.g. List.ContentTypes)
                                publicTypeRecursive = (fieldExpressionBody as MethodCallExpression).Type;
                            }

                            //var publicTypeRecursive = (fieldExpressionBody as MethodCallExpression).Type.GenericTypeArguments[0];
                            var entityInfoRecursive = Instance.GetStaticClassInfo(publicTypeRecursive);

                            var expandedEntityField = new EntityFieldExpandInfo() { Name = fld, Type = publicTypeRecursive, Expandable = true };
                            ParseQueryProperties(entityInfo, (includeFieldExpression as UnaryExpression).Operand as LambdaExpression, expandedEntityField);

                            // Add key field if needed
                            var keyFieldRecursive = expandedEntityField.Fields.FirstOrDefault(p => p.Name == entityInfoRecursive.ActualKeyFieldName);
                            if (keyFieldRecursive == null)
                            {
                                expandedEntityField.Fields.Add(new EntityFieldExpandInfo() { Name = entityInfoRecursive.ActualKeyFieldName });
                            }

                            entityFieldExpandInfo.Fields.Add(expandedEntityField);
                        }
                    }
                }
            }

            if (first)
            {
                // Add key field if needed
                var keyFieldNonRecursive = entityFieldExpandInfo.Fields.FirstOrDefault(p => p.Name == resultTypeInfo.ActualKeyFieldName);
                if (keyFieldNonRecursive == null)
                {
                    entityFieldExpandInfo.Fields.Add(new EntityFieldExpandInfo() { Name = resultTypeInfo.ActualKeyFieldName });
                }

                var fieldToUpdate = entityInfo.Fields.FirstOrDefault(p => p.Name == fieldToLoad);

                if (fieldToUpdate != null)
                {
                    fieldToUpdate.ExpandFieldInfo = entityFieldExpandInfo;
                }
            }

            return fieldToLoad;
        }

        internal Expression<Func<object, object>>[] GetEntityKeyExpressions(IDataModelParent entity)
        {
            var entityType = entity.GetType();
            var entityInfo = GetStaticClassInfo(entityType);

            if (string.IsNullOrEmpty(entityInfo.ActualKeyFieldName))
            {
                throw new ApplicationException(string.Format(PnPCoreResources.Exception_InvalidDomainModelConfiguration, entityType.FullName));
            }

            var parameter = Expression.Parameter(entityType, "i");
            var objectParameter = Expression.Parameter(typeof(object), "i");
            var body = Expression.Convert(Expression.PropertyOrField(parameter, entityInfo.ActualKeyFieldName), typeof(object));
            var expression = Expression.Lambda(body, parameter);
            var objectExpression = Expression.Lambda<Func<object, object>>(expression.Body, objectParameter);

            var result = new Expression<Func<object, object>>[] { objectExpression };

            return (result);
        }

        #region Private implementation

        private static Type GetEntityConcreteType(Type type)
        {
            // Translate any provided interface type into the corresponding concrete type
            if (type.IsInterface)
            {
                var concreteType = type.GetCustomAttribute<ConcreteTypeAttribute>();
                if (concreteType != null)
                {
                    type = concreteType.Type;
                }
                else
                {
                    throw new ClientException(ErrorType.ModelMetadataIncorrect, string.Format(
                        PnPCoreResources.Exception_ModelMetadataIncorrect_MissingConcreteTypeAttribute, type.Name));
                }
            }

            return type;
        }

        private static EntityFieldInfo EnsureClassField(Type type, PropertyInfo property, EntityInfo classInfo)
        {
            var classField = classInfo.Fields.FirstOrDefault(p => p.Name.Equals(property.Name, StringComparison.InvariantCultureIgnoreCase));
            if (classField == null)
            {
                classField = new EntityFieldInfo()
                {
                    Name = property.Name,
                    DataType = property.PropertyType,
                    PropertyInfo = type.GetProperty(property.Name),
                };
                classInfo.Fields.Add(classField);
            }

            if (classInfo.SharePointTargets.Any() && string.IsNullOrEmpty(classField.SharePointName))
            {
                // This type can be loaded via SharePoint REST, so ensure the SharePoint field is populated
                classField.SharePointName = property.Name;
            }

            return classField;
        }

        private static string ToCamelCase(string str)
        {
            if (!string.IsNullOrEmpty(str) && str.Length > 1)
            {
                return char.ToLowerInvariant(str[0]) + str.Substring(1);
            }
            return str;
        }

        #endregion

        #region Replication logic

        /// <summary>
        /// Replicates Key and metadata between two domain model objects
        /// </summary>
        /// <param name="sourceObject">The source domain model object</param>
        /// <param name="destinationObject">The destination domain model object</param>
        internal static void ReplicateKeyAndMetadata(IDataModelParent sourceObject, IDataModelParent destinationObject)
        {
            // Copy the Key of the original parent into the replicated parent
            if (sourceObject is IDataModelWithKey keySource &&
                destinationObject is IDataModelWithKey keyDestination)
            {
                try
                {
                    keyDestination.Key = keySource.Key;
                }
                catch (Exception)
                {
                    // We intentionally ignore any exception 
                    // in case the source key is missing 
                }
            }

            // Copy original parent metadata to the replicated parent metadata, if any
            if (sourceObject is IMetadataExtensible metadataSource &&
                destinationObject is IMetadataExtensible metadataDestination)
            {
                foreach (var md in metadataSource.Metadata)
                {
                    metadataDestination.Metadata[md.Key] = md.Value;
                }
            }
        }

        internal static IDataModelParent ReplicateParentHierarchy(IDataModelParent parent, PnPContext context)
        {
            if (parent == null)
            {
                throw new ArgumentNullException(nameof(parent));
            }

            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            IDataModelParent result;

            // If the parent is a collection
            if (parent is IManageableCollection)
            {
                IDataModelParent collectionSuperParent = null;

                // Being the parent a collection, I need to replicate its parent, if any, too
                if (parent.Parent != null)
                {
                    collectionSuperParent = ReplicateParentHierarchy(parent.Parent, context);
                }

                // We create a new parent collection, replicating all the properties from the other
                result = ReplicateParentCollection(parent, collectionSuperParent, context);
            }
            else
            {
                // Otherwise we just replicate the parent
                result = ReplicateParent(parent, context);

                if (parent.Parent != null)
                {
                    result.Parent = ReplicateParentHierarchy(parent.Parent, context);
                }
            }

            return result;
        }

        internal static IDataModelParent ReplicateParent(IDataModelParent parent, PnPContext context)
        {
            if (parent == null)
            {
                throw new ArgumentNullException(nameof(parent));
            }

            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var parentType = parent.GetType();

            // Create a new instance of Parent with the same data type as the original parent
            var replicatedParent = (IDataModelParent)GetEntityConcreteInstance(parentType, null, context);

            ReplicateKeyAndMetadata(parent, replicatedParent);

            return replicatedParent;
        }

        internal static IDataModelParent ReplicateParentCollection(IDataModelParent parent, IDataModelParent superParent, PnPContext context)
        {
            if (parent == null)
            {
                throw new ArgumentNullException(nameof(parent));
            }

            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            // TODO: This is a bit risky ... we should fine something better ...
            var collectionPropertyName = parent.GetType().Name.Replace("Collection", "s");

            // We create a new parent collection, but replicating all the properties from the other
            var replicatedParentCollection = (IDataModelParent)GetEntityCollectionInstance(parent.GetType(), context, superParent, collectionPropertyName);

            ReplicateKeyAndMetadata(parent, replicatedParentCollection);

            return replicatedParentCollection;
        }

        #endregion
    }
}
