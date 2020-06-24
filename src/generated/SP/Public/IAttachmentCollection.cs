using System.Linq;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a collection of Attachment objects
    /// </summary>
    public interface IAttachmentCollection : IQueryable<IAttachment>, IDataModelCollection<IAttachment>
    {
    }
}