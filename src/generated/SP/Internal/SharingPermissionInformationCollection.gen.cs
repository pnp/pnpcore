using PnP.Core.QueryModel.Model;
using PnP.Core.Services;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Collection of SharingPermissionInformation Domain Model objects
    /// </summary>
    internal partial class SharingPermissionInformationCollection : QueryableDataModelCollection<ISharingPermissionInformation>, ISharingPermissionInformationCollection
    {
        public SharingPermissionInformationCollection(PnPContext context, IDataModelParent parent) : base(context, parent)
        {
            this.PnPContext = context;
            this.Parent = parent;
        }
    }
}