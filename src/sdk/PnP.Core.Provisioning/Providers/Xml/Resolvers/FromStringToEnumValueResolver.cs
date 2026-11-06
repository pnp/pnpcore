using System;

namespace PnP.Core.Provisioning.Providers.Xml.Resolvers
{
    /// <summary>
    /// Resolves a Decimal value into a Double
    /// </summary>
    internal class FromStringToEnumValueResolver : IValueResolver
    {
        public string Name => this.GetType().Name;

        private readonly Type _targetItemType;

        public FromStringToEnumValueResolver(Type targetItemType)
        {
            _targetItemType = targetItemType;
        }

        public object Resolve(object source, object destination, object sourceValue)
        {
            var s = sourceValue != null ? sourceValue.ToString() : null;
            return !string.IsNullOrEmpty(s) ? Enum.Parse(_targetItemType, s, true) : 0;
        }
    }
}
