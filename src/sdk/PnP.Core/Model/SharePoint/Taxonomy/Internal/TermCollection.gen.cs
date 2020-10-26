using PnP.Core.Services;

namespace PnP.Core.Model.SharePoint
{
    internal partial class TermCollection : BaseDataModelCollection<ITerm>, ITermCollection
    {
        public TermCollection(PnPContext context, IDataModelParent parent)
        {
            this.PnPContext = context;
            this.Parent = parent;
        }
    }
}
