using PnP.Core.QueryModel;
using PnP.Core.Services;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Collection of Folder Domain Model objects
    /// </summary>
    internal partial class FolderCollection : QueryableDataModelCollection<IFolder>, IFolderCollection
    {
        public FolderCollection(PnPContext context, IDataModelParent parent, string memberName = null) : base(context, parent, memberName)
        {
            this.PnPContext = context;
            this.Parent = parent;
        }
    }
}