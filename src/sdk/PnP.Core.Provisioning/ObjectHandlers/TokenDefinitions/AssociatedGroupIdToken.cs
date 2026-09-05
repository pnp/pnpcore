using PnP.Core.Provisioning.Attributes;
using PnP.Core.Services;
using System.Threading.Tasks;
using PnP.Core.Model.SharePoint;

namespace PnP.Core.Provisioning.ObjectHandlers.TokenDefinitions
{
    [TokenDefinitionDescription(
       Token = "{associatedownergroupid}",
       Description = "Returns the id of the associated owners SharePoint group of a site",
       Example = "{associatedownergroupid}",
       Returns = "3")]
    [TokenDefinitionDescription(
       Token = "{associatedmembergroupid}",
       Description = "Returns the id of the associated members SharePoint group of a site",
       Example = "{associatedmembergroupid}",
       Returns = "4")]
    [TokenDefinitionDescription(
       Token = "{associatedvisitorgroupid}",
       Description = "Returns the id of the associated visitors SharePoint group of a site",
       Example = "{associatedvisitorgroupid}",
       Returns = "5")]
    internal class AssociatedGroupIdToken : VolatileTokenDefinition
    {
        private readonly AssociatedGroupType _groupType;

        public AssociatedGroupIdToken(PnPContext context, AssociatedGroupType groupType)
            : base(context, $"{{associated{groupType.ToString().TrimEnd('s')}groupid}}")
        {
            _groupType = groupType;
        }

        public override async Task<string> GetReplaceValueAsync()
        {
            if (string.IsNullOrEmpty(CacheValue))
            {
                switch (_groupType)
                {
                    case AssociatedGroupType.owners:
                        {
                            IWeb web = await Context.Web.GetAsync(w => w.AssociatedOwnerGroup).ConfigureAwait(false);
                            CacheValue = web.AssociatedOwnerGroup?.Id.ToString();
                            break;
                        }
                    case AssociatedGroupType.members:
                        {
                            IWeb web = await Context.Web.GetAsync(w => w.AssociatedMemberGroup).ConfigureAwait(false);
                            CacheValue = web.AssociatedMemberGroup?.Id.ToString();
                            break;
                        }
                    case AssociatedGroupType.visitors:
                        {
                            IWeb web = await Context.Web.GetAsync(w => w.AssociatedVisitorGroup).ConfigureAwait(false);
                            CacheValue = web.AssociatedVisitorGroup?.Id.ToString();
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