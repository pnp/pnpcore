using System.Collections.Generic;

namespace PnP.Core.Services.Core.CSOM.Utils
{
    class CSOMBodySerializer : IBodySerializer
    {
        public string SerializeRequestBody(List<ActionObjectPath> requests)
        {
            string actions = "";
            string identities = "";
            foreach (var request in requests)
            {
                if (request.Action != null)
                {
                    actions += request.Action.ToString();
                }
                if (request.ObjectPath != null)
                {
                    identities += request.ObjectPath.ToString();
                }
            }
            return $"<Request AddExpandoFieldTypeSuffix=\"true\" SchemaVersion=\"15.0.0.0\" LibraryVersion=\"16.0.0.0\" ApplicationName=\"pnp core sdk\" xmlns=\"http://schemas.microsoft.com/sharepoint/clientquery/2009\"><Actions>{actions}</Actions><ObjectPaths>{identities}</ObjectPaths></Request>";
        }
    }
}
