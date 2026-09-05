using PnP.Core.Provisioning.Attributes;
using System;
using PnP.Core.Services;
using System.Threading.Tasks;

namespace PnP.Core.Provisioning.ObjectHandlers.TokenDefinitions
{
    [TokenDefinitionDescription(
    Token = "{sequencesitegroupid:[provisioningid]}",
    Description = "Returns a Id of the associated group or an empty guid if not O365 has been associated with this site",
    Example = "{sequencesitegroupid:MYID}",
    Returns = "c7d9f9aa-4696-4c27-8a22-7d8eb7e70fda")]
    internal class SequenceSiteGroupIdToken : TokenDefinition
    {
        private Guid _id = Guid.Empty;
        public SequenceSiteGroupIdToken(PnPContext context, string provisioningId, Guid id)
            : base(context, $"{{sequencesitegroupid:{provisioningId}}}")
        {
            _id = id;
        }

        public override Task<string> GetReplaceValueAsync()
        {
            return Task.FromResult<string>(_id.ToString());
        }
    }
}
