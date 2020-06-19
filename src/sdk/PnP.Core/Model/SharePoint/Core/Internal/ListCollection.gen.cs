using PnP.Core.QueryModel.Model;
using PnP.Core.Services;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Collection of List Domain Model objects
    /// </summary>
    internal partial class ListCollection : QueryableDataModelCollection<IList>, IListCollection
    {
        public ListCollection(PnPContext context, IDataModelParent parent, string memberName = null)
            : base(context, parent, memberName)
        {
            this.PnPContext = context;
            this.Parent = parent;
        }
    }
}
