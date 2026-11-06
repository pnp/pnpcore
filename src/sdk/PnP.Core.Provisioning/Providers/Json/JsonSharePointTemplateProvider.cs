using PnP.Core.Services;
using PnP.Core.Provisioning.Model;
using PnP.Core.Provisioning.Connectors;


namespace PnP.Core.Provisioning.Providers.Json
{
    public class JsonSharePointTemplateProvider : JsonTemplateProvider
    {
        public JsonSharePointTemplateProvider() : base()
        {

        }

        public JsonSharePointTemplateProvider(PnPContext context, string connectionString, string container) :
            base(new SharePointConnector(context, connectionString, container))
        {
        }
    }
}
