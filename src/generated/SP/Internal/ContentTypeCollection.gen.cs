using PnP.Core.QueryModel.Model;
using PnP.Core.Services;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Collection of ContentType Domain Model objects
    /// </summary>
    internal partial class ContentTypeCollection : QueryableDataModelCollection<IContentType>, IContentTypeCollection
    {
        public ContentTypeCollection(PnPContext context, IDataModelParent parent) : base(context, parent)
        {
            this.PnPContext = context;
            this.Parent = parent;
        }
    }
}