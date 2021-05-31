using PnP.Core.Model.Security;
using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Indicates a change to a <seealso cref="IList"/> object.
    /// </summary>
    /// <seealso cref="PnP.Core.Model.SharePoint.IChange" />
    [ConcreteType(typeof(ChangeList))]
    public interface IChangeList : IChange
    {
        public string Editor { get; }
        public bool Hidden { get; }

        /// <summary>
        /// Gets a value that identifies the changed list.
        /// </summary>
        public Guid ListId { get; }

        /// <summary>
        /// Gets a value that specifies the root folder URL of the the changed list.
        /// </summary>
        public string RootFolderUrl { get; }

        /// <summary>
        /// Gets a value that specifies the template type of the the changed list.
        /// </summary>
        public ListTemplateType TemplateType { get; }

        /// <summary>
        /// Gets a value that specifies the title of the the changed list.
        /// </summary>
        public string Title { get; }

        /// <summary>
        /// Gets a value that identifies the web that contains the changed list.
        /// </summary>
        public Guid WebId { get; }

        /// <summary>
        /// Gets a value that specifies the creator of the the changed list.
        /// </summary>
        public ISharePointUser Creator { get; }
    }
}