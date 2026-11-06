using PnP.Core.Provisioning.Utilities;
using System;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace PnP.Core.Provisioning.Providers.Xml.Resolvers
{
    /// <summary>
    /// Resolves a Dictionary into an Array of objects
    /// </summary>
    internal class XmlAnyFromSchemaToModelValueResolver : IValueResolver
    {
        public string Name => this.GetType().Name;

        private readonly String _elementName;

        public XmlAnyFromSchemaToModelValueResolver(String elementName)
        {
            this._elementName = elementName;
        }

        public object Resolve(object source, object destination, object sourceValue)
        {
            XElement result = null;

            var xmlAny = sourceValue.GetPublicInstancePropertyValue("Any") as XmlElement[];

            if (null != xmlAny)
            {
                result = new XElement(this._elementName,
                    from x in xmlAny select x.ToXElement());
            }

            return (result);
        }
    }
}
