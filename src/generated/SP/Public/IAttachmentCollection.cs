using PnP.Core.Services;
using System.Linq;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a collection of Attachment objects
    /// </summary>
    [ConcreteType(typeof(AttachmentCollection))]
    public interface IAttachmentCollection : IQueryable<IAttachment>, IDataModelCollection<IAttachment>
    {
    }
}