using PnP.Core.Provisioning.Connectors;

namespace PnP.Core.Provisioning.Providers.Xml
{
    public class XMLFileSystemTemplateProvider : XMLTemplateProvider
    {

        public XMLFileSystemTemplateProvider() : base()
        {
        }

        public XMLFileSystemTemplateProvider(string connectionString, string container) :
            base(new FileSystemConnector(connectionString, container))
        {
        }
    }
}
