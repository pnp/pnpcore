using PnP.Core.QueryModel;
using PnP.Core.Services;

namespace PnP.Core.Model.SharePoint
{
    internal partial class ContentTypeCollection : QueryableDataModelCollection<IContentType>, IContentTypeCollection
    {
        public ContentTypeCollection(PnPContext context, IDataModelParent parent, string memberName = null)
            : base(context, parent, memberName)
        {
            this.PnPContext = context;
            this.Parent = parent;
        }

    }
}
