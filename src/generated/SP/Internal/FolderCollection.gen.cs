using PnP.Core.QueryModel.Model;
using PnP.Core.Services;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Collection of Folder Domain Model objects
    /// </summary>
    internal partial class FolderCollection : QueryableDataModelCollection<IFolder>, IFolderCollection
    {
        public FolderCollection(PnPContext context, IDataModelParent parent) : base(context, parent)
        {
            this.PnPContext = context;
            this.Parent = parent;
        }
    }
}