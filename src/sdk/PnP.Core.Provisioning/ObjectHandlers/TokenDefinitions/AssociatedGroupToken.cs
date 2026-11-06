using PnP.Core.Provisioning.Attributes;
using PnP.Core.Services;
using System.Threading.Tasks;
using PnP.Core.Model.SharePoint;

namespace PnP.Core.Provisioning.ObjectHandlers.TokenDefinitions
{
    [TokenDefinitionDescription(
       Token = "{associatedownergroup}",
       Description = "Returns the title of the associated owners SharePoint group of a site",
       Example = "{associatedownergroup}",
       Returns = "My Site Owners Group Title")]
    [TokenDefinitionDescription(
       Token = "{associatedmembergroup}",
       Description = "Returns the title of the associated members SharePoint group of a site",
       Example = "{associatedmembergroup}",
       Returns = "My Site Members Group Title")]
    [TokenDefinitionDescription(
       Token = "{associatedvisitorgroup}",
       Description = "Returns the title of the associated visitors SharePoint group of a site",
       Example = "{associatedvisitorgroup}",
       Returns = "My Site Visitors Group Title")]
    internal class AssociatedGroupToken : VolatileTokenDefinition
    {
        private AssociatedGroupType _groupType;

        public AssociatedGroupToken(PnPContext context, AssociatedGroupType groupType)
            : base(context, $"{{associated{groupType.ToString().TrimEnd('s')}group}}")
        {
            _groupType = groupType;

            IsCacheable = false;
        }

        internal AssociatedGroupType GroupType { get => _groupType; set => _groupType = value; }

        public override async Task<string> GetReplaceValueAsync()
        {
            if (string.IsNullOrEmpty(CacheValue))
            {
                switch (_groupType)
                {
                    case AssociatedGroupType.owners:
                        {
                            IWeb web = await Context.Web.GetAsync(w => w.AssociatedOwnerGroup).ConfigureAwait(false);
                            CacheValue = web.AssociatedOwnerGroup?.Title;
                            break;
                        }
                    case AssociatedGroupType.members:
                        {
                            IWeb web = await Context.Web.GetAsync(w => w.AssociatedMemberGroup).ConfigureAwait(false);
                            CacheValue = web.AssociatedMemberGroup?.Title;
                            break;
                        }
                    case AssociatedGroupType.visitors:
                        {
                            IWeb web = await Context.Web.GetAsync(w => w.AssociatedVisitorGroup).ConfigureAwait(false);
                            CacheValue = web.AssociatedVisitorGroup?.Title;
                            break;
                        }
                }
            }
            return CacheValue;
        }

        public enum AssociatedGroupType
        {
            owners,
            members,
            visitors
        }
    }
}
