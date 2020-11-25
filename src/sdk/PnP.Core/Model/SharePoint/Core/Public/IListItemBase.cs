using PnP.Core.Services;
using System.Threading.Tasks;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Base interface for a list item, used by <see cref="IListItem"/> and <see cref="IPage"/>
    /// </summary>
    public interface IListItemBase
    {
        /// <summary>
        /// Id of the list item
        /// </summary>
        public int Id { get; }        

        /// <summary>
        /// Title value of the list item
        /// </summary>
        public string Title { get; set; }

        #region Extension methods

        #region SystemUpdate

        /// <summary>
        /// Performs a system update of the list item
        /// </summary>
        public Task SystemUpdateAsync();

        /// <summary>
        /// Performs a system update of the list item
        /// </summary>
        public void SystemUpdate();

        /// <summary>
        /// Performs a system update of the list item
        /// </summary>
        public Task SystemUpdateBatchAsync();

        /// <summary>
        /// Performs a system update of the list item
        /// </summary>
        public void SystemUpdateBatch();

        /// <summary>
        /// Performs a system update of the list item
        /// </summary>
        /// <param name="batch">Batch to add the systemupdate request to</param>
        public Task SystemUpdateBatchAsync(Batch batch);

        /// <summary>
        /// Performs a system update of the list item
        /// </summary>
        /// <param name="batch">Batch to add the systemupdate request to</param>
        public void SystemUpdateBatch(Batch batch);

        #endregion

        #region UpdateOverwriteVersion

        /// <summary>
        /// Performs a UpdateOverwriteVersion of the list item
        /// </summary>
        public Task UpdateOverwriteVersionAsync();

        /// <summary>
        /// Performs a UpdateOverwriteVersion of the list item
        /// </summary>
        public void UpdateOverwriteVersion();

        /// <summary>
        /// Performs a UpdateOverwriteVersion of the list item
        /// </summary>
        public Task UpdateOverwriteVersionBatchAsync();

        /// <summary>
        /// Performs a UpdateOverwriteVersion of the list item
        /// </summary>
        public void UpdateOverwriteVersionBatch();

        /// <summary>
        /// Performs a UpdateOverwriteVersion of the list item
        /// </summary>
        /// <param name="batch">Batch to add the UpdateOverwriteVersion request to</param>
        public Task UpdateOverwriteVersionBatchAsync(Batch batch);

        /// <summary>
        /// Performs a UpdateOverwriteVersion of the list item
        /// </summary>
        /// <param name="batch">Batch to add the UpdateOverwriteVersion request to</param>
        public void UpdateOverwriteVersionBatch(Batch batch);

        #endregion

        #region Comments handling
        /// <summary>
        /// Are comments disabled for this <see cref="IListItem"/>?
        /// </summary>
        /// <returns>True if disabled, false otherwise</returns>
        public Task<bool> AreCommentsDisabledAsync();

        /// <summary>
        /// Are comments disabled for this <see cref="IListItem"/>?
        /// </summary>
        /// <returns>True if disabled, false otherwise</returns>
        public bool AreCommentsDisabled();

        /// <summary>
        /// Enable/Disable comments for this list item
        /// </summary>
        /// <param name="commentsDisabled">Do comments need to enabled or disabled</param>
        /// <returns></returns>
        public Task SetCommentsDisabledAsync(bool commentsDisabled);

        /// <summary>
        /// Enable/Disable comments for this list item
        /// </summary>
        /// <param name="commentsDisabled">Do comments need to enabled or disabled</param>
        /// <returns></returns>
        public void SetCommentsDisabled(bool commentsDisabled);
        #endregion

        #endregion

    }
}
