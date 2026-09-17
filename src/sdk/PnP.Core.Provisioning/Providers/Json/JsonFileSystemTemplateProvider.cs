using PnP.Core.Provisioning.Connectors;

namespace PnP.Core.Provisioning.Providers.Json
{
    public class JsonFileSystemTemplateProvider : JsonTemplateProvider
    {
        public JsonFileSystemTemplateProvider() : base()
        {

        }

        public JsonFileSystemTemplateProvider(string connectionString, string container) :
            base(new FileSystemConnector(connectionString, container))
        {
        }
    }
}
