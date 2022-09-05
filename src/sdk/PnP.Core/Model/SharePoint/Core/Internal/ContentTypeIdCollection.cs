using PnP.Core.QueryModel;
using PnP.Core.Services;

namespace PnP.Core.Model.SharePoint
{
    internal sealed class ContentTypeIdCollection : QueryableDataModelCollection<IContentTypeId>, IContentTypeIdCollection
    {
        public ContentTypeIdCollection(PnPContext context, IDataModelParent parent, string memberName = null)
            : base(context, parent, memberName)
        {
            PnPContext = context;
            Parent = parent;
        }
    }
}
