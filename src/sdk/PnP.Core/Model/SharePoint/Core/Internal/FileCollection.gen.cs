using PnP.Core.QueryModel;
using PnP.Core.Services;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Collection of File Domain Model objects
    /// </summary>
    internal partial class FileCollection : QueryableDataModelCollection<IFile>, IFileCollection
    {
        public FileCollection(PnPContext context, IDataModelParent parent, string memberName) : base(context, parent, memberName)
        {
            this.PnPContext = context;
            this.Parent = parent;
        }
    }
}