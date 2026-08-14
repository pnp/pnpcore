using PnP.Core.Provisioning.Utilities;
using System;
using System.Linq;
using System.Xml.Linq;

namespace PnP.Core.Provisioning.Providers.Xml.Resolvers
{
    /// <summary>
    /// Resolves a Dictionary into an Array of objects
    /// </summary>
    internal class XmlAnyFromModelToSchemalValueResolver : IValueResolver
    {
        public string Name => this.GetType().Name;

        private readonly Type elementType;

        public XmlAnyFromModelToSchemalValueResolver(Type elementType)
        {
            this.elementType = elementType;
        }

        public object Resolve(object source, object destination, object sourceValue)
        {
            object res = null;

            if ((sourceValue != null) && (sourceValue is XElement))
            {
                var any = ((XElement)sourceValue).Elements().Select(x => x.ToXmlElement()).ToArray();
                res = Activator.CreateInstance(this.elementType, true);
                res.GetPublicInstanceProperty("Any").SetValue(res, any);
            }
            return res;
        }
    }
}
