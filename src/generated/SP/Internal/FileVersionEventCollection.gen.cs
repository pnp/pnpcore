using PnP.Core.QueryModel.Model;
using PnP.Core.Services;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Collection of FileVersionEvent Domain Model objects
    /// </summary>
    internal partial class FileVersionEventCollection : QueryableDataModelCollection<IFileVersionEvent>, IFileVersionEventCollection
    {
        public FileVersionEventCollection(PnPContext context, IDataModelParent parent) : base(context, parent)
        {
            this.PnPContext = context;
            this.Parent = parent;
        }
    }
}