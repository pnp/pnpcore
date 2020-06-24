using PnP.Core.QueryModel.Model;
using PnP.Core.Services;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Collection of RecycleBinItem Domain Model objects
    /// </summary>
    internal partial class RecycleBinItemCollection : QueryableDataModelCollection<IRecycleBinItem>, IRecycleBinItemCollection
    {
        public RecycleBinItemCollection(PnPContext context, IDataModelParent parent) : base(context, parent)
        {
            this.PnPContext = context;
            this.Parent = parent;
        }
    }
}