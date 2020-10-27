using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a RecycleBinItem object
    /// </summary>
    [ConcreteType(typeof(RecycleBinItem))]
    public interface IRecycleBinItem : IDataModel<IRecycleBinItem>, IDataModelGet<IRecycleBinItem>, IDataModelUpdate, IDataModelDelete
    {

        #region Existing properties

        /// <summary>
        /// To update...
        /// </summary>
        public string AuthorEmail { get; }

        /// <summary>
        /// To update...
        /// </summary>
        public string AuthorName { get; }

        /// <summary>
        /// To update...
        /// </summary>
        public string DeletedByEmail { get; }

        /// <summary>
        /// To update...
        /// </summary>
        public string DeletedByName { get; }

        /// <summary>
        /// To update...
        /// </summary>
        public DateTime DeletedDate { get; }

        /// <summary>
        /// To update...
        /// </summary>
        public string DeletedDateLocalFormatted { get; }

        /// <summary>
        /// To update...
        /// </summary>
        public string DirName { get; }

        /// <summary>
        /// To update...
        /// </summary>
        public Guid Id { get; }

        /// <summary>
        /// To update...
        /// </summary>
        public int ItemState { get; }

        /// <summary>
        /// To update...
        /// </summary>
        public int ItemType { get; }

        /// <summary>
        /// To update...
        /// </summary>
        public string LeafName { get; }

        /// <summary>
        /// To update...
        /// </summary>
        public string Title { get; }

        /// <summary>
        /// To update...
        /// </summary>
        public IUser Author { get; }

        /// <summary>
        /// To update...
        /// </summary>
        public IUser DeletedBy { get; }

        #endregion

        #region New properties

        #endregion

    }
}
