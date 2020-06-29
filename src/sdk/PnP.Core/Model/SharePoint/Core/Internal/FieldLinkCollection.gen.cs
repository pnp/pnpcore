using PnP.Core.QueryModel.Model;
using PnP.Core.Services;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Collection of FieldLink Domain Model objects
    /// </summary>
    internal partial class FieldLinkCollection : QueryableDataModelCollection<IFieldLink>, IFieldLinkCollection
    {
        public FieldLinkCollection(PnPContext context, IDataModelParent parent, string memberName = null)
            : base(context, parent, memberName)
        {
            this.PnPContext = context;
            this.Parent = parent;
        }
    }
}
