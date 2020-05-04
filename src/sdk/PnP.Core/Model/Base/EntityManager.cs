using PnP.Core.Model.Base;
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
                var sharePointTypeAttribute = type.GetCustomAttribute<SharePointTypeAttribute>(false);
                var graphTypeAttribute = type.GetCustomAttribute<GraphTypeAttribute>(false);
                if (sharePointTypeAttribute != null || graphTypeAttribute != null)
                {
                    EntityInfo classInfo = new EntityInfo
                    {
                        UseOverflowField = type.ImplementsInterface(typeof(IExpandoDataModel))
                    };

                    if (sharePointTypeAttribute != null)
                    {
                        classInfo.SharePointType = sharePointTypeAttribute.Type;
                        classInfo.SharePointUri = sharePointTypeAttribute.Uri;
                        classInfo.SharePointGet = !string.IsNullOrEmpty(sharePointTypeAttribute.Get) ? sharePointTypeAttribute.Get : sharePointTypeAttribute.Uri;
                        classInfo.SharePointLinqGet = !string.IsNullOrEmpty(sharePointTypeAttribute.LinqGet) ? sharePointTypeAttribute.LinqGet : sharePointTypeAttribute.Uri;
                        classInfo.SharePointOverflowProperty = sharePointTypeAttribute.OverflowProperty;
                        classInfo.SharePointUpdate = !string.IsNullOrEmpty(sharePointTypeAttribute.Update) ? sharePointTypeAttribute.Update : sharePointTypeAttribute.Update;
                        classInfo.SharePointDelete = !string.IsNullOrEmpty(sharePointTypeAttribute.Delete) ? sharePointTypeAttribute.Delete : sharePointTypeAttribute.Delete;
                    }

                    if (graphTypeAttribute != null)
                    {
                        classInfo.GraphId = !string.IsNullOrEmpty(graphTypeAttribute.Id) ? graphTypeAttribute.Id : "id";
                        classInfo.GraphGet = !string.IsNullOrEmpty(graphTypeAttribute.Get) ? graphTypeAttribute.Get : graphTypeAttribute.Uri;
                        classInfo.GraphLinqGet = !string.IsNullOrEmpty(graphTypeAttribute.LinqGet) ? graphTypeAttribute.Get : graphTypeAttribute.Uri;
                        classInfo.GraphOverflowProperty = graphTypeAttribute.OverflowProperty;
                        classInfo.GraphUpdate = !string.IsNullOrEmpty(graphTypeAttribute.Update) ? graphTypeAttribute.Update : graphTypeAttribute.Uri;
                        classInfo.GraphDelete = !string.IsNullOrEmpty(graphTypeAttribute.Delete) ? graphTypeAttribute.Delete : graphTypeAttribute.Uri;
                        classInfo.GraphBeta = graphTypeAttribute.Beta;
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
                                        classField.SharePointExpandable = sharePointPropertyAttribute.Expandable;
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

                        if (classField == null && !skipField)
                        {
                            classField = EnsureClassField(type, property, classInfo);
                            // Property was not decorated with attributes
                            if (string.IsNullOrEmpty(classInfo.SharePointType))
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
                    }

                    // Find the property set as key field and mark it as such
                    if (!string.IsNullOrEmpty(keyPropertyName))
                    {
                        var keyProperty = classInfo.Fields.FirstOrDefault(p => p.Name == keyPropertyName);
                        if (keyProperty != null)
                        {
                            if (!string.IsNullOrEmpty(classInfo.SharePointType))
                            {
                                keyProperty.IsSharePointKey = true;
                            }

                            keyProperty.IsGraphKey = true;
                            // If a property is defined as graph key then ensure the GraphName is correctly set
                            if (string.IsNullOrEmpty(keyProperty.GraphName))
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

                        if (string.IsNullOrEmpty(classInfo.SharePointType))
                        {
                            // This is a Graph only property
                            overflowField.GraphName = classInfo.GraphOverflowProperty;
                        }
                        else
                        {
                            // This is SharePoint/Graph property
                            overflowField.SharePointName = classInfo.SharePointOverflowProperty;
                            overflowField.GraphName = classInfo.GraphOverflowProperty;
                        }
                    }

                    // Add to our cache to speed up future retrievals
                    entityCache.TryAdd(type, classInfo);
                    return classInfo;

                }
                else
                {
                    throw new Exception("Each domain model object must be decorated with a ClassMapping attribute");
                }
            }
        }
        
        /// <summary>
        /// Creates a concrete instance of a domain model type based on the reference type
        /// </summary>
        /// <param name="type">The reference model type, can be an interface or a class</param>
        /// <returns>Entity model class describing this model instance</returns>
        internal object GetEntityConcreteInstance<TModel>(Type type)
        {
            if (type is null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            type = GetEntityConcreteType(type);

            return (TModel)Activator.CreateInstance(type);
        }

        /// <summary>
        /// Creates a concrete instance of a domain model type based on the reference type
        /// </summary>
        /// <param name="type">The reference model type, can be an interface or a class</param>
        /// <returns>Entity model class describing this model instance</returns>
        internal TransientObject GetEntityConcreteInstance(Type type)
        {
            if (type is null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            type = GetEntityConcreteType(type);

            return (TransientObject)Activator.CreateInstance(type);
        }

        /// <summary>
        /// Translates model into a set of classes that are used to drive CRUD operations, this takes into account the passed expressions
        /// </summary>
        /// <param name="modelType">The Type of the model object to process</param>
        /// <param name="expressions">Data load expressions</param>
        /// <returns>Entity model class describing this model instance</returns>
        internal EntityInfo GetClassInfo<TModel>(Type modelType, params Expression<Func<TModel, object>>[] expressions)
        {
            // Get static information about the fields to work with and how to handle CRUD operations
            var staticClassInfo = EntityManager.Instance.GetStaticClassInfo(modelType);

            // Copy static info in dynamic model to allow instance specific updates
            EntityInfo entityInfo = new EntityInfo(staticClassInfo);

            // if the user specified an expression then update our class info to reflect that 
            if (expressions != null && expressions.Any())
            {
                var nonExpandableGraphCollections = entityInfo.GraphNonExpandableCollections;

                List<string> graphFieldsToLoad = new List<string>();
                List<string> sharePointFieldsToLoad = new List<string>();
                foreach (Expression<Func<TModel, object>> expression in expressions)
                {
                    string fieldToLoad = null;
                    if (expression.Body is MemberExpression)
                    {
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

                    // Non expandable collections do not count as regular field as they're loaded via an additional query
                    if (nonExpandableGraphCollections.Any())
                    {
                        if (nonExpandableGraphCollections.FirstOrDefault(p => p.Name.Equals(fieldToLoad, StringComparison.InvariantCultureIgnoreCase)) == null)
                        {
                            graphFieldsToLoad.Add(fieldToLoad);
                        }
                    }
                    else
                    {
                        graphFieldsToLoad.Add(fieldToLoad);
                    }

                    sharePointFieldsToLoad.Add(fieldToLoad);
                }

                if (graphFieldsToLoad.Count > 0)
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

            return entityInfo;
        }

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
            }

            return type;
        }

        private static EntityFieldInfo EnsureClassField(Type type, PropertyInfo property, EntityInfo classInfo)
        {
            var classField = classInfo.Fields.FirstOrDefault(p => p.Name.Equals(property.Name, StringComparison.InvariantCultureIgnoreCase));
            if (classField == null)
            {
                try
                {
                    classField = new EntityFieldInfo()
                    {
                        Name = property.Name,
                        DataType = property.PropertyType,
                        PropertyInfo = type.GetProperty(property.Name),
                    };
                    classInfo.Fields.Add(classField);
                }
                catch (Exception ex)
                {
                    var t = ex.Message;
                }
            }

            if (!string.IsNullOrEmpty(classInfo.SharePointType))
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
                return Char.ToLowerInvariant(str[0]) + str.Substring(1);
            }
            return str;
        }
    }
}
