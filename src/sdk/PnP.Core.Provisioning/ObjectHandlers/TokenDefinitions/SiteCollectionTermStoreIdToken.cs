using PnP.Core.Provisioning.Attributes;
using PnP.Core.Services;
using System.Threading.Tasks;
using PnP.Core.Model.SharePoint;

namespace PnP.Core.Provisioning.ObjectHandlers.TokenDefinitions
{
    [TokenDefinitionDescription(
      Token = "{sitecollectiontermstoreid}",
      Description = "Returns the id of the default site collection term store",
      Example = "{sitecollectiontermstoreid}",
      Returns = "f2cd6d5b-1391-480e-a3dc-7f7f96137382")]
    internal class SiteCollectionTermStoreIdToken : TokenDefinition
    {
        public SiteCollectionTermStoreIdToken(PnPContext context)
            : base(context, "{sitecollectiontermstoreid}")
        {
        }

        public override async Task<string> GetReplaceValueAsync()
        {
            if (CacheValue == null)
            {
                ITermStore termStore = await Context.TermStore.GetAsync(t => t.Id).ConfigureAwait(false);
                CacheValue = termStore.Id;
            }
            return CacheValue;
        }
    }
}