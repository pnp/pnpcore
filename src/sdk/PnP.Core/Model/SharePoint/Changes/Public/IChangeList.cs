using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Indicates a change to a <seealso cref="IList"/> object.
    /// </summary>
    /// <seealso cref="IChange" />
    [ConcreteType(typeof(ChangeList))]
    public interface IChangeList : IChange
    {
        /// <summary>
        /// A string that returns the name of the person who modified the list.
        /// </summary>
        public string Editor { get; }

        /// <summary>
        /// Returns a Boolean value that indicates whether a list is a hidden list.
        /// </summary>
        public bool Hidden { get; }

        /// <summary>
        /// Gets a value that identifies the changed list.
        /// </summary>
        public Guid ListId { get; }

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

    }
}