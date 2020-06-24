using PnP.Core.QueryModel.Model;
using PnP.Core.Services;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Collection of Attachment Domain Model objects
    /// </summary>
    internal partial class AttachmentCollection : QueryableDataModelCollection<IAttachment>, IAttachmentCollection
    {
        public AttachmentCollection(PnPContext context, IDataModelParent parent) : base(context, parent)
        {
            this.PnPContext = context;
            this.Parent = parent;
        }
    }
}