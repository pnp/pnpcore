using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Collection of list item attachments
    /// </summary>
    [ConcreteType(typeof(AttachmentCollection))]
    public interface IAttachmentCollection : IQueryable<IAttachment>, IDataModelCollection<IAttachment>
    {
        /// <summary>
        /// Adds a list item attachment
        /// </summary>
        /// <param name="name">Name of the list item attachment</param>
        /// <param name="content">File contents</param>
        /// <returns>The added list item attachment</returns>
        Task<IAttachment> AddAsync(string name, Stream content);

        /// <summary>
        /// Adds a list item attachment
        /// </summary>
        /// <param name="name">Name of the list item attachment</param>
        /// <param name="content">File contents</param>
        /// <returns>The added list item attachment</returns>
        IAttachment Add(string name, Stream content);
    }
}