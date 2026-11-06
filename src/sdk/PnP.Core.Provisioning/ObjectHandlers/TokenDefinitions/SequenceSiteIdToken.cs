using PnP.Core.Provisioning.Attributes;
using System;
using PnP.Core.Services;
using System.Threading.Tasks;

namespace PnP.Core.Provisioning.ObjectHandlers.TokenDefinitions
{
    [TokenDefinitionDescription(
    Token = "{sequencesiteid:[provisioningid]}",
    Description = "Returns a id of the site given its provisioning ID from the sequence",
    Example = "{sequencesiteid:MYID}",
    Returns = "https://contoso.sharepoint.com/sites/mynewsite")]
    internal class SequenceSiteIdToken : TokenDefinition
    {
        private Guid _id = Guid.Empty;
        public SequenceSiteIdToken(PnPContext context, string provisioningId, Guid id)
            : base(context, $"{{sequencesiteid:{provisioningId}}}")
        {
            _id = id;
        }

        public override Task<string> GetReplaceValueAsync()
        {
            return Task.FromResult<string>(_id.ToString());
        }
    }
}
