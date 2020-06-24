using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a ChangeItem object
    /// </summary>
    [ConcreteType(typeof(ChangeItem))]
    public interface IChangeItem : IDataModel<IChangeItem>, IDataModelUpdate, IDataModelDelete
    {

        #region New properties

        /// <summary>
        /// To update...
        /// </summary>
        public int ActivityType { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string Editor { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string EditorEmailHint { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string EditorLoginName { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public int FileSystemObjectType { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string FileType { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string Hashtag { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool Hidden { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public int ItemId { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public Guid ListId { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public int ListTemplate { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string ListTitle { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string ServerRelativeUrl { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public Guid UniqueId { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public Guid WebId { get; set; }

        #endregion

    }
}
