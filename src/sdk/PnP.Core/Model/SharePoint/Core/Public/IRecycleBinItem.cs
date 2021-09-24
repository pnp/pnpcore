using PnP.Core.Model.Security;
using PnP.Core.Services;
using System;
using System.Threading.Tasks;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a RecycleBinItem object
    /// </summary>
    [ConcreteType(typeof(RecycleBinItem))]
    public interface IRecycleBinItem : IDataModel<IRecycleBinItem>, IDataModelGet<IRecycleBinItem>, IDataModelLoad<IRecycleBinItem>, IDataModelUpdate, IDataModelDelete, IQueryableDataModel
    {
        /// <summary>
        /// Gets the e-mail address of the user who created the recycle bin item.
        /// </summary>
        public string AuthorEmail { get; }

        /// <summary>
        /// Gets the name of the user who created the recycle bin item.
        /// </summary>
        public string AuthorName { get; }

        /// <summary>
        /// Gets the e-mail address of the user who deleted the recycle bin item.
        /// </summary>
        public string DeletedByEmail { get; }

        /// <summary>
        /// Gets the name of the user who deleted the recycle bin item.
        /// </summary>
        public string DeletedByName { get; }

        /// <summary>
        /// Gets a value that specifies when the Recycle Bin item was moved to the Recycle Bin.
        /// </summary>
        public DateTime DeletedDate { get; }

        /// <summary>
        /// Gets a value that specifies when the Recycle Bin item was moved to the Recycle Bin formatted in the local date.
        /// </summary>
        public string DeletedDateLocalFormatted { get; }

        /// <summary>
        /// Gets a value that specifies the site relative URL of the list or folder that originally contained the Recycle Bin item.
        /// </summary>
        public string DirName { get; }

        /// <summary>
        /// Gets a value that specifies the identifier of the Recycle Bin item.
        /// </summary>
        public Guid Id { get; }

        /// <summary>
        /// Gets a value that specifies the Recycle Bin stage of the Recycle Bin item.
        /// </summary>
        public RecycleBinItemState ItemState { get; }

        /// <summary>
        /// Gets a value that specifies the type of the Recycle Bin item.
        /// </summary>
        public RecycleBinItemType ItemType { get; }

        /// <summary>
        /// Gets a value that specifies the leaf name of the Recycle Bin item.
        /// </summary>
        public string LeafName { get; }

        /// <summary>
        /// Gets a value that specifies the title of the Recycle Bin item.
        /// </summary>
        public string Title { get; }

        /// <summary>
        /// Gets a value that specifies the size of the Recycle Bin item in bytes.
        /// </summary>
        public long Size { get; }

        /// <summary>
        /// Gets a value that specifies the user who created the Recycle Bin item.
        /// </summary>
        public ISharePointUser Author { get; }

        /// <summary>
        /// Gets a value that specifies the user who deleted the Recycle Bin item.
        /// </summary>
        public ISharePointUser DeletedBy { get; }

        /// <summary>
        /// A special property used to add an asterisk to a $select statement
        /// </summary>
        public object All { get; }

        #region Restore
        /// <summary>
        /// Restores the recycle bin item to its original location.
        /// </summary>
        public Task RestoreAsync();

        /// <summary>
        /// Restores the recycle bin item to its original location.
        /// </summary>
        public void Restore();

        /// <summary>
        /// Restores the recycle bin item to its original location using a specific batch instance
        /// <param name="batch">The batch instance to use.</param>
        /// </summary>
        public Task RestoreBatchAsync(Batch batch);

        /// <summary>
        /// Restores the recycle bin item to its original location using a specific batch instance
        /// <param name="batch">The batch instance to use.</param>
        /// </summary>
        public void RestoreBatch(Batch batch);

        /// <summary>
        /// Restores the recycle bin item to its original location using the current context batch instance
        /// </summary>
        public Task RestoreBatchAsync();

        /// <summary>
        /// Restores the recycle bin item to its original location using the current context batch instance
        /// </summary>
        public void RestoreBatch();
        #endregion

        #region MoveToSecondStage
        /// <summary>
        /// Move the recycle bin item to the second stage recycle bin
        /// </summary>
        public Task MoveToSecondStageAsync();

        /// <summary>
        /// Move the recycle bin item to the second stage recycle bin
        /// </summary>
        public void MoveToSecondStage();

        /// <summary>
        /// Move the recycle bin item to the second stage recycle bin using a specific batch instance
        /// <param name="batch">The batch instance to use.</param>
        /// </summary>
        public Task MoveToSecondStageBatchAsync(Batch batch);

        /// <summary>
        /// Move the recycle bin item to the second stage recycle bin using a specific batch instance
        /// <param name="batch">The batch instance to use.</param>
        /// </summary>
        public void MoveToSecondStageBatch(Batch batch);

        /// <summary>
        /// Move the recycle bin item to the second stage recycle bin using the current context batch instance
        /// </summary>
        public Task MoveToSecondStageBatchAsync();

        /// <summary>
        /// Move the recycle bin item to the second stage recycle bin using the current context batch instance
        /// </summary>
        public void MoveToSecondStageBatch();
        #endregion
    }
}
