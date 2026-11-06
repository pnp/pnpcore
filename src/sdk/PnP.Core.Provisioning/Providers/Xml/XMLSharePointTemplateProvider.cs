using PnP.Core.Services;
using PnP.Core.Provisioning.Model;
using PnP.Core.Provisioning.Connectors;

namespace PnP.Core.Provisioning.Providers.Xml
{
    public class XMLSharePointTemplateProvider : XMLTemplateProvider
    {

        public XMLSharePointTemplateProvider() : base()
        {
        }

        public XMLSharePointTemplateProvider(PnPContext context, string connectionString, string container) :
            base(new SharePointConnector(context, connectionString, container))
        {
        }
    }
}
