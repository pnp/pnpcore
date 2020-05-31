using PnP.Core.QueryModel.Model;
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

        public override IContentType CreateNew()
        {
            return NewContentType();
        }

        private ContentType AddNewContentType()
        {
            var newContentType = NewContentType();
            this.items.Add(newContentType);
            return newContentType;
        }

        private ContentType NewContentType()
        {
            var newContentType = new ContentType
            {
                PnPContext = this.PnPContext,
                Parent = this,
            };
            return newContentType;
        }
    }
}
