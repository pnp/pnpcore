using System;
using System.Threading.Tasks;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Syntex model
    /// </summary>
    public interface ISyntexModel
    {
        /// <summary>
        /// Id of a model (= id of the list item)
        /// </summary>
        public int Id { get; }

        /// <summary>
        /// Unique id of the model (= unique id of the model file)
        /// </summary>
        public Guid UniqueId { get; }

        /// <summary>
        /// Name of the model
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Model description
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// Date when the model was trained for the last time
        /// </summary>
        public DateTime ModelLastTrained { get; }

        /// <summary>
        /// File holding the classifier
        /// </summary>
        public IListItem ListItem { get; }

        /// <summary>
        /// Register this model on a list
        /// </summary>
        /// <param name="list">List to register this model on</param>
        /// <returns></returns>
        public Task RegisterModelAsync(IList list);
    }
}
