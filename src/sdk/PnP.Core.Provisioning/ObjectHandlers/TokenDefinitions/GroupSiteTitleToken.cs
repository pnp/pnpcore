using PnP.Core.Provisioning.Attributes;
using PnP.Core.Services;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using PnP.Core.Model.SharePoint;

namespace PnP.Core.Provisioning.ObjectHandlers.TokenDefinitions
{
    [TokenDefinitionDescription(
      Token = "{groupsitetitle}",
      Description = "Returns the title of the current site, with characters that are illegal in a Microsoft 365 group alias replaced by underscores",
      Example = "{groupsitetitle}",
      Returns = "My_Site")]
    [TokenDefinitionDescription(
      Token = "{groupsitename}",
      Description = "Returns the title of the current site, with characters that are illegal in a Microsoft 365 group alias replaced by underscores",
      Example = "{groupsitename}",
      Returns = "My_Site")]
    internal class GroupSiteTitleToken : VolatileTokenDefinition
    {
        public GroupSiteTitleToken(PnPContext context) : base(context, "{groupsitetitle}", "{groupsitename}")
        {
        }

        public override async Task<string> GetReplaceValueAsync()
        {
            if (CacheValue == null)
            {
                IWeb web = await Context.Web.GetAsync(w => w.Title).ConfigureAwait(false);
                CacheValue = Regex.Replace(web.Title, "[\"/\\[\\]\\\\:|<>+=;,?*'@]", "_");
            }
            return CacheValue;
        }
    }
}