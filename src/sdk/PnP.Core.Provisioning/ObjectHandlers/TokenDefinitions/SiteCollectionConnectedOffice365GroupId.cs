using PnP.Core.Provisioning.Attributes;
using PnP.Core.Services;
using System;
using System.Threading.Tasks;
using PnP.Core.Model.SharePoint;

namespace PnP.Core.Provisioning.ObjectHandlers.TokenDefinitions
{
    [TokenDefinitionDescription(
      Token = "{sitecollectionconnectedoffice365groupid}",
      Description = "Returns the id of the Microsoft 365 group connected to the current site collection",
      Example = "{sitecollectionconnectedoffice365groupid}",
      Returns = "f2cd6d5b-1391-480e-a3dc-7f7f96137382")]
    internal class SiteCollectionConnectedOffice365GroupId : VolatileTokenDefinition
    {
        public SiteCollectionConnectedOffice365GroupId(PnPContext context)
            : base(context, "{sitecollectionconnectedoffice365groupid}")
        {
        }

        public override async Task<string> GetReplaceValueAsync()
        {
            if (CacheValue == null)
            {
                ISite site = await Context.Site.GetAsync(s => s.GroupId, s => s.RelatedGroupId).ConfigureAwait(false);

                if (!site.GroupId.Equals(Guid.Empty))
                {
                    CacheValue = site.GroupId.ToString();
                }
                else if (!site.RelatedGroupId.Equals(Guid.Empty))
                {
                    CacheValue = site.RelatedGroupId.ToString();
                }
                else
                {
                    CacheValue = string.Empty;
                }
            }
            return CacheValue;
        }
    }
}