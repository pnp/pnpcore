using PnP.Core.Services;
using System.Threading.Tasks;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a SharePoint Online list item
    /// </summary>
    [ConcreteType(typeof(ListItem))]
    public interface IListItem : IDataModel<IListItem>, IDataModelUpdate, IDataModelDelete, IExpandoDataModel
    {
        /// <summary>
        /// Id of the list item
        /// </summary>
        public int Id { get; }

        /// <summary>
        /// Can comments be added to this list item
        /// </summary>
        public bool CommentsDisabled { get; set; }

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
        public Task SystemUpdateBatchAsync();

        /// <summary>
        /// Performs a system update of the list item
        /// </summary>
        /// <param name="batch">Batch to add the systemupdate request to</param>
        public Task SystemUpdateBatchAsync(Batch batch);

        #endregion

        #region UpdateOverwriteVersion

        /// <summary>
        /// Performs a UpdateOverwriteVersion of the list item
        /// </summary>
        public Task UpdateOverwriteVersionAsync();

        /// <summary>
        /// Performs a UpdateOverwriteVersion of the list item
        /// </summary>
        public Task UpdateOverwriteVersionBatchAsync();

        /// <summary>
        /// Performs a UpdateOverwriteVersion of the list item
        /// </summary>
        /// <param name="batch">Batch to add the UpdateOverwriteVersion request to</param>
        public Task UpdateOverwriteVersionBatchAsync(Batch batch);

        #endregion

        #endregion
    }
}
