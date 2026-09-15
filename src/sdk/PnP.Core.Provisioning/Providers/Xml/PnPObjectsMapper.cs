using PnP.Core.Provisioning.Model;
using PnP.Core.Provisioning.Providers.Xml.Resolvers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace PnP.Core.Provisioning.Providers.Xml
{
    /// <summary>
    /// Utility class that maps one object to another
    /// </summary>
    internal static class PnPObjectsMapper
    {

        #region MapProperties

        /// <summary>
        /// Maps the properties of a typed source object, to the properties of an untyped destination object
        /// </summary>
        /// <typeparam name="TSource">The type of the source object</typeparam>
        /// <param name="source">The source object</param>
        /// <param name="destination">The destination object</param>
        /// <param name="resolverExpressions">Any custom resolver, optional</param>
        /// <param name="recursive">Defines whether to apply the mapping recursively, optional and by default false</param>
        public static void MapProperties<TSource>(TSource source, Object destination, Dictionary<Expression<Func<TSource, Object>>, IResolver> resolverExpressions = null, Boolean recursive = false)
        {
            Dictionary<string, IResolver> resolvers = ConvertExpressionsToResolvers(resolverExpressions);
            MapProperties(source, destination, resolvers, recursive);
        }

        /// <summary>
        /// Maps the properties of an untyped source object object, to the properties of a typed destination object
        /// </summary>
        /// <typeparam name="TDestination">The type of the destination object</typeparam>
        /// <param name="source">The source object</param>
        /// <param name="destination">The destination object</param>
        /// <param name="resolverExpressions">Any custom resolver, optional</param>
        /// <param name="recursive">Defines whether to apply the mapping recursively, optional and by default false</param>
        public static void MapProperties<TDestination>(Object source, TDestination destination, Dictionary<Expression<Func<TDestination, Object>>, IResolver> resolverExpressions = null, Boolean recursive = false)
        {
            Dictionary<string, IResolver> resolvers = ConvertExpressionsToResolvers(resolverExpressions);
            MapProperties(source, destination, resolvers, recursive);
        }

        /// <summary>
        /// Maps the properties of a source object, to the properties of a destination object
        /// </summary>
        /// <param name="source">The source object</param>
        /// <param name="destination">The destination object</param>
        /// <param name="resolvers">Any custom resolver, optional</param>
        /// <param name="recursive">Defines whether to apply the mapping recursively, optional and by default false</param>
        public static void MapProperties(Object source, Object destination, Dictionary<String, IResolver> resolvers = null, Boolean recursive = false)
        {
            var destinationProperties = destination?.GetType().GetProperties(
                System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.Public);

            var sourceProperties = source?.GetType().GetProperties(
                System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.Public);

            if (null != resolvers)
            {
                resolvers = resolvers.ToDictionary(i => i.Key.ToUpper(), i => i.Value);
            }

            var filteredProperties = destinationProperties?.Where(
                p => (!Attribute.IsDefined(p, typeof(ObsoleteAttribute)) &&
                (p.PropertyType.BaseType.Name != typeof(BaseProvisioningTemplateObjectCollection<>).Name || recursive) &&
                (p.PropertyType.BaseType.Name != typeof(BaseProvisioningHierarchyObjectCollection<>).Name || recursive) &&
                (!p.PropertyType.IsArray || recursive) // &&
                ));
            foreach (var dp in filteredProperties) // TODO: Think about this rule ...
            {
                var resolverKey = $"{dp.DeclaringType.FullName}.{dp.Name}".ToUpper();
                var resolver = resolvers != null && resolvers.ContainsKey(resolverKey) ? resolvers[resolverKey] : null;

                var sp = sourceProperties?.FirstOrDefault(p => p.Name.Equals(dp.Name, StringComparison.InvariantCultureIgnoreCase));
                var spSpecified = sourceProperties?.FirstOrDefault(p => p.Name.Equals($"{dp.Name}Specified", StringComparison.InvariantCultureIgnoreCase));
                var dpSpecified = destinationProperties?.FirstOrDefault(p => p.Name.Equals($"{dp.Name}Specified", StringComparison.InvariantCultureIgnoreCase));
                if (null != sp || null != resolver)
                {
                    if (null != resolver)
                    {
                        if (resolver is IValueResolver)
                        {
                            dp.SetValue(destination, ((IValueResolver)resolver)
                                .Resolve(source, destination, sp?.GetValue(source)));
                        }
                        else if (resolver is ITypeResolver)
                        {
                            if (!((ITypeResolver)resolver).CustomCollectionResolver &&
                                (dp.PropertyType.BaseType.Name == typeof(BaseProvisioningTemplateObjectCollection<>).Name ||
                                dp.PropertyType.BaseType.Name == typeof(BaseProvisioningHierarchyObjectCollection<>).Name))
                            {
                                var destinationCollection = dp.GetValue(destination);
                                if (destinationCollection != null)
                                {
                                    var resolvedCollection = ((ITypeResolver)resolver)
                                        .Resolve(source, resolvers, recursive);

                                    destinationCollection.GetType().GetMethod("AddRange",
                                        System.Reflection.BindingFlags.Instance |
                                        System.Reflection.BindingFlags.Public |
                                        System.Reflection.BindingFlags.IgnoreCase)
                                        .Invoke(destinationCollection, new Object[] { resolvedCollection });
                                }
                            }
                            else
                            {
                                dp.SetValue(destination, ((ITypeResolver)resolver)
                                    .Resolve(source, resolvers, recursive));
                            }
                        }
                    }
                    else if (null != sp)
                    {
                        try
                        {
                            if (recursive && (dp.PropertyType.BaseType.Name == typeof(BaseProvisioningTemplateObjectCollection<>).Name ||
                                dp.PropertyType.BaseType.Name == typeof(BaseProvisioningHierarchyObjectCollection<>).Name))
                            {
                                var destinationCollection = dp.GetValue(destination);
                                if (destinationCollection != null)
                                {
                                    var resolvedCollection =
                                        PnPObjectsMapper.MapObjects(sp.GetValue(source),
                                        new CollectionFromSchemaToModelTypeResolver(
                                            dp.PropertyType.BaseType.GenericTypeArguments[0]), resolvers, recursive);

                                    destinationCollection.GetType().GetMethod("AddRange",
                                        System.Reflection.BindingFlags.Instance |
                                        System.Reflection.BindingFlags.Public |
                                        System.Reflection.BindingFlags.IgnoreCase)
                                        .Invoke(destinationCollection, new Object[] { resolvedCollection });
                                }
                            }
                            else if (recursive && dp.PropertyType.IsArray)
                            {
                                dp.SetValue(destination,
                                        PnPObjectsMapper.MapObjects(sp.GetValue(source),
                                            new CollectionFromModelToSchemaTypeResolver(dp.PropertyType.IsArray ? dp.PropertyType.GetElementType() : null),
                                            resolvers, recursive));
                            }
                            else
                            {
                                object sourceValue = sp.GetValue(source);
                                if (sourceValue != null && dp.PropertyType == typeof(string) && sp.PropertyType != typeof(string))
                                {
                                    sourceValue = sourceValue.ToString();
                                }
                                else if (sourceValue != null && dp.PropertyType == typeof(int) && sp.PropertyType != typeof(int))
                                {
                                    sourceValue = Int32.Parse(sourceValue.ToString());
                                }
                                else if (sourceValue != null && dp.PropertyType == typeof(bool) && sp.PropertyType != typeof(bool))
                                {
                                    sourceValue = Boolean.Parse(sourceValue.ToString());
                                }
                                else if (sourceValue != null && dp.PropertyType.IsEnum)
                                {
                                    sourceValue = Enum.Parse(dp.PropertyType, sourceValue.ToString());
                                }
                                else if (sourceValue != null && dp.PropertyType.Name == "Nullable`1" && dp.PropertyType.GenericTypeArguments[0].IsEnum)
                                {
                                    sourceValue = Enum.Parse(dp.PropertyType.GenericTypeArguments[0], sourceValue.ToString());
                                }
                                else if (sourceValue == null &&
                                    dp.ReflectedType.Namespace == typeof(ProvisioningTemplate).Namespace &&
                                    dp.GetValue(destination) != null)
                                {
                                    sourceValue = dp.GetValue(destination);
                                }
                                else if (sourceValue != null && spSpecified != null)
                                {
                                    bool isSpecified = (bool)spSpecified.GetValue(source);
                                    if (!isSpecified)
                                    {
                                        sourceValue = null;
                                    }
                                }
                                dp.SetValue(destination, sourceValue);

                                if (dpSpecified != null)
                                {
                                    dpSpecified.SetValue(destination, true);
                                }
                            }
                        }
                        catch (Exception)
                        {
                        }
                    }
                }
            }
        }

        #endregion

        #region MapObjects

        /// <summary>
        /// Maps a source object, into a destination object
        /// </summary>
        /// <typeparam name="TDestination">The type of the destination object</typeparam>
        /// <param name="source">The source object</param>
        /// <param name="resolver">A custom resolver</param>
        /// <param name="resolverExpressions">Any custom resolver, optional</param>
        /// <param name="recursive">Defines whether to apply the mapping recursively, optional and by default false</param>
        /// <returns>The mapped destination object</returns>
        public static Object MapObjects<TDestination>(Object source, ITypeResolver resolver, Dictionary<Expression<Func<TDestination, Object>>, IResolver> resolverExpressions = null, Boolean recursive = false)
        {
            Dictionary<string, IResolver> resolvers = ConvertExpressionsToResolvers(resolverExpressions);
            return (MapObjects(source, resolver, resolvers, recursive));
        }

        /// <summary>
        /// Maps a source object, into a destination object
        /// </summary>
        /// <param name="source">The source object</param>
        /// <param name="resolver">A custom resolver</param>
        /// <param name="resolvers">Any custom resolver, optional</param>
        /// <param name="recursive">Defines whether to apply the mapping recursively, optional and by default false</param>
        /// <returns>The mapped destination object</returns>
        public static Object MapObjects(Object source, ITypeResolver resolver, Dictionary<String, IResolver> resolvers = null, Boolean recursive = false)
        {
            Object result = null;

            if (null != resolver)
            {
                result = resolver.Resolve(source, resolvers, recursive);
            }

            return (result);
        }

        #endregion

        #region Utility methods

        /// <summary>
        /// Transforms a Dictionary of IValueResolver instances by Expression into a Dictionary by String (property name)
        /// </summary>
        /// <typeparam name="TTarget">The target Type of the expression</typeparam>
        /// <param name="resolverExpressions">The Dictionary to transform</param>
        /// <returns>The transformed dictionary</returns>
        private static Dictionary<String, IResolver> ConvertExpressionsToResolvers<TTarget>(Dictionary<Expression<Func<TTarget, object>>, IResolver> resolverExpressions)
        {
            Dictionary<String, IResolver> resolvers = null;

            if (resolverExpressions != null)
            {
                resolvers = new Dictionary<String, IResolver>();

                foreach (var re in resolverExpressions.Keys)
                {
                    var propertySelector = re.Body as MemberExpression ?? ((UnaryExpression)re.Body).Operand as MemberExpression;
                    resolvers.Add($"{propertySelector.Member.DeclaringType.FullName}.{propertySelector.Member.Name}".ToUpper(), resolverExpressions[re]);
                }
            }

            return resolvers;
        }

        #endregion
    }
}
