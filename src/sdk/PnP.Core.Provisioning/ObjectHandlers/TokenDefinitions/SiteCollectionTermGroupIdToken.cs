using PnP.Core.Model.SharePoint;
using PnP.Core.Provisioning.Attributes;
using PnP.Core.Services;
using System.Linq;
using System.Threading.Tasks;

namespace PnP.Core.Provisioning.ObjectHandlers.TokenDefinitions
{
    [TokenDefinitionDescription(
      Token = "{sitecollectiontermgroupid}",
      Description = "Returns the id of the term group associated with the current site collection",
      Example = "{sitecollectiontermgroupid}",
      Returns = "f2cd6d5b-1391-480e-a3dc-7f7f96137382")]
    internal class SiteCollectionTermGroupIdToken : VolatileTokenDefinition
    {
        public SiteCollectionTermGroupIdToken(PnPContext context)
            : base(context, "{sitecollectiontermgroupid}")
        {
        }

        public override async Task<string> GetReplaceValueAsync()
        {
            if (CacheValue == null)
            {
                CacheValue = (await SiteCollectionTermGroupResolver
                    .GetAsync(Context).ConfigureAwait(false))?.Id ?? string.Empty;
            }
            return CacheValue;
        }
    }
}