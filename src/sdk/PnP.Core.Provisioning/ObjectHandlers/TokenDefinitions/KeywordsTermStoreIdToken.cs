using PnP.Core.Provisioning.Attributes;
using PnP.Core.Services;
using System.Threading.Tasks;
using PnP.Core.Model.SharePoint;

namespace PnP.Core.Provisioning.ObjectHandlers.TokenDefinitions
{
    [TokenDefinitionDescription(
      Token = "{keywordstermstoreid}",
      Description = "Returns the id of the default keywords term store",
      Example = "{keywordstermstoreid}",
      Returns = "f2cd6d5b-1391-480e-a3dc-7f7f96137382")]
    internal class KeywordsTermStoreIdToken : TokenDefinition
    {
        public KeywordsTermStoreIdToken(PnPContext context)
            : base(context, "{keywordstermstoreid}")
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