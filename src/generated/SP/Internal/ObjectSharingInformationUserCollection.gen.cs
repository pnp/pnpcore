using PnP.Core.QueryModel.Model;
using PnP.Core.Services;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Collection of ObjectSharingInformationUser Domain Model objects
    /// </summary>
    internal partial class ObjectSharingInformationUserCollection : QueryableDataModelCollection<IObjectSharingInformationUser>, IObjectSharingInformationUserCollection
    {
        public ObjectSharingInformationUserCollection(PnPContext context, IDataModelParent parent) : base(context, parent)
        {
            this.PnPContext = context;
            this.Parent = parent;
        }
    }
}