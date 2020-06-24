using PnP.Core.QueryModel.Model;
using PnP.Core.Services;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Collection of File Domain Model objects
    /// </summary>
    internal partial class FileCollection : QueryableDataModelCollection<IFile>, IFileCollection
    {
        public FileCollection(PnPContext context, IDataModelParent parent) : base(context, parent)
        {
            this.PnPContext = context;
            this.Parent = parent;
        }
    }
}