using PnP.Core.Provisioning.Attributes;
using System;
using PnP.Core.Services;
using System.Threading.Tasks;

namespace PnP.Core.Provisioning.ObjectHandlers.TokenDefinitions
{
    [TokenDefinitionDescription(
    Token = "{sequencesitecollectionid:[provisioningid]}",
    Description = "Returns a site collection id of the site given its provisioning ID from the sequence",
    Example = "{sequencesitecollectionid:MYID}",
    Returns = "https://contoso.sharepoint.com/sites/mynewsite")]
    internal class SequenceSiteCollectionIdToken : TokenDefinition
    {
        private Guid _id = Guid.Empty;
        public SequenceSiteCollectionIdToken(PnPContext context, string provisioningId, Guid id)
            : base(context, $"{{sequencesitecollectionid:{provisioningId}}}")
        {
            _id = id;
        }

        public override Task<string> GetReplaceValueAsync()
        {
            return Task.FromResult<string>(_id.ToString());
        }
    }
}
