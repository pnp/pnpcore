using PnP.Core.Provisioning.Utilities;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace PnP.Core.Provisioning.Providers.Xml.Resolvers
{
    /// <summary>
    /// Typed vesion of PropertyObjectTypeResolver
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class PropertyObjectTypeResolver<T> : PropertyObjectTypeResolver
    {
        public PropertyObjectTypeResolver(Expression<Func<T, object>> exp, Func<object, object> sourceValueSelector = null, ITypeResolver resolver = null) :
            base(GetPropertyType(exp), GetPropertyName(exp), sourceValueSelector, resolver)
        {
        }

        private static string GetPropertyName(Expression<Func<T, object>> exp)
        {
            return (exp.Body as MemberExpression ?? ((UnaryExpression)exp.Body).Operand as MemberExpression).Member.Name;
        }

        private static Type GetPropertyType(Expression<Func<T, object>> exp)
        {
            return (exp.Body as MemberExpression ?? ((UnaryExpression)exp.Body).Operand as MemberExpression).Type;
        }
    }

    /// <summary>
    /// Resolves a collection type from Domain Model to Schema
    /// </summary>
    internal class PropertyObjectTypeResolver : ITypeResolver
    {
        public string Name => this.GetType().Name;
        public bool CustomCollectionResolver => false;

        private readonly Type targetItemType;
        private readonly string propertyName;
        private readonly Func<object, object> sourceValueSelector = null;
        private readonly ITypeResolver typeResolver = null;

        public PropertyObjectTypeResolver(Type targetItemType, string propertyName, Func<object, object> sourceValueSelector = null, ITypeResolver typeResolver = null)
        {
            this.targetItemType = targetItemType;
            this.propertyName = propertyName;
            this.sourceValueSelector = sourceValueSelector;
            this.typeResolver = typeResolver;
        }

        public object Resolve(object source, Dictionary<String, IResolver> resolvers = null, Boolean recursive = false)
        {
            var sourcePropertyValue = (this.sourceValueSelector == null) ? source.GetPublicInstancePropertyValue(propertyName) :
                this.sourceValueSelector(source);
            if (null != sourcePropertyValue)
            {
                object result = null;
                if (this.typeResolver == null)
                {
                    result = Activator.CreateInstance(this.targetItemType, true);
                    PnPObjectsMapper.MapProperties(sourcePropertyValue, result, resolvers, recursive);
                }
                else
                {
                    result = PnPObjectsMapper.MapObjects(sourcePropertyValue, typeResolver, resolvers, recursive);
                }
                return (result);
            }
            else
            {
                return (null);
            }
        }
    }
}
