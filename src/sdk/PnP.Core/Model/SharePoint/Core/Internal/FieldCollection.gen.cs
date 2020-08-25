using PnP.Core.QueryModel;
using PnP.Core.Services;

namespace PnP.Core.Model.SharePoint
{
    internal partial class FieldCollection : QueryableDataModelCollection<IField>, IFieldCollection
    {
        public FieldCollection(PnPContext context, IDataModelParent parent, string memberName = null)
            : base(context, parent, memberName)
        {
            this.PnPContext = context;
            this.Parent = parent;
        }
    }
}
