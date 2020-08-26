using PnP.Core.Services;

namespace PnP.Core.Model.SharePoint
{
    internal partial class TermRelationCollection : BaseDataModelCollection<ITermRelation>, ITermRelationCollection
    {
        public TermRelationCollection(PnPContext context, IDataModelParent parent)
        {
            this.PnPContext = context;
            this.Parent = parent;
        }
    }
}
