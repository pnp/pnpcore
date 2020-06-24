using PnP.Core.QueryModel.Model;
using PnP.Core.Services;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Collection of Alert Domain Model objects
    /// </summary>
    internal partial class AlertCollection : QueryableDataModelCollection<IAlert>, IAlertCollection
    {
        public AlertCollection(PnPContext context, IDataModelParent parent) : base(context, parent)
        {
            this.PnPContext = context;
            this.Parent = parent;
        }
    }
}