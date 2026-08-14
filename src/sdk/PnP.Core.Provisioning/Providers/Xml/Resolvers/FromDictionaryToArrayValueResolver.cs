using PnP.Core.Provisioning.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace PnP.Core.Provisioning.Providers.Xml.Resolvers
{
    /// <summary>
    /// Resolves a Dictionary into an Array of objects
    /// </summary>
    internal class FromDictionaryToArrayValueResolver<TKey, TValue> : IValueResolver
    {
        public string Name => this.GetType().Name;

        private readonly String _keyField;
        private readonly String _valueField;
        private readonly Type _targetArrayItemType;
        private readonly String _sourcePropertyName;

        public FromDictionaryToArrayValueResolver(Type targetArrayItemType,
            LambdaExpression keySelector, LambdaExpression valueSelector, string sourcePropertyName = null)
        {
            this._targetArrayItemType = targetArrayItemType;

            var keyField = keySelector.Body as MemberExpression ?? ((UnaryExpression)keySelector.Body).Operand as MemberExpression;
            var valueField = valueSelector.Body as MemberExpression ?? ((UnaryExpression)valueSelector.Body).Operand as MemberExpression;

            this._keyField = keyField.Member.Name;
            this._valueField = valueField.Member.Name;
            this._sourcePropertyName = sourcePropertyName;
        }

        public object Resolve(object source, object destination, object sourceValue)
        {
            object result = null;

            if (null == sourceValue && null != source && !string.IsNullOrEmpty(_sourcePropertyName))
            {
                sourceValue = source.GetPublicInstancePropertyValue(_sourcePropertyName);
            }

            var sourceDictionary = sourceValue != null && sourceValue is IEnumerable<KeyValuePair<TKey, TValue>> ?
                sourceValue as IEnumerable<KeyValuePair<TKey, TValue>> :
                source as IEnumerable<KeyValuePair<TKey, TValue>>;

            if (null == sourceDictionary && null != sourceValue)
            {
                throw new ArgumentException("Invalid source object. Expected type implementing IEnumerable<KeyValuePair<TKey, TValue>>", nameof(source));
            }
            else if (null != sourceDictionary && sourceDictionary.Any())
            {
                var listType = typeof(List<>);
                var resultType = this._targetArrayItemType.MakeArrayType();

                var resultArray = (Array)Activator.CreateInstance(resultType, sourceDictionary.Count());
                var i = 0;
                foreach (var item in sourceDictionary)
                {
                    var resultItem = Activator.CreateInstance(this._targetArrayItemType);
                    resultItem.GetType().GetProperty(this._keyField, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public).SetValue(resultItem, item.Key);
                    resultItem.GetType().GetProperty(this._valueField, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public).SetValue(resultItem, item.Value);
                    resultArray.SetValue(resultItem, i++);
                }
                result = resultArray;
            }

            return (result);
        }
    }
}
