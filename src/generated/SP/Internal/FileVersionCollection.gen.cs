using PnP.Core.QueryModel.Model;
using PnP.Core.Services;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Collection of FileVersion Domain Model objects
    /// </summary>
    internal partial class FileVersionCollection : QueryableDataModelCollection<IFileVersion>, IFileVersionCollection
    {
        public FileVersionCollection(PnPContext context, IDataModelParent parent) : base(context, parent)
        {
            this.PnPContext = context;
            this.Parent = parent;
        }
    }
}