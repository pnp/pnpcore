using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a RecycleBinItem object
    /// </summary>
    [ConcreteType(typeof(RecycleBinItem))]
    public interface IRecycleBinItem : IDataModel<IRecycleBinItem>, IDataModelUpdate, IDataModelDelete
    {

        #region New properties

        /// <summary>
        /// To update...
        /// </summary>
        public string AuthorEmail { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string AuthorName { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string DeletedByEmail { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string DeletedByName { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public DateTime DeletedDate { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string DeletedDateLocalFormatted { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string DirName { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public int ItemState { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public int ItemType { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string LeafName { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public IUser Author { get; }

        /// <summary>
        /// To update...
        /// </summary>
        public IUser DeletedBy { get; }

        #endregion

    }
}
