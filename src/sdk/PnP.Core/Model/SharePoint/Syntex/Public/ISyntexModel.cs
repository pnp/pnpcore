using System;
using System.Threading.Tasks;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Syntex model
    /// </summary>
    public interface ISyntexModel
    {
        #region Properties
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
        #endregion

        #region Methods
        /// <summary>
        /// Publish this model to a library
        /// </summary>
        /// <param name="list">List to publish this model to</param>
        /// <param name="viewOption">Determines how adding the model changes the library's views</param>
        /// <returns>Information about the model publication</returns>
        public Task<ISyntexModelPublicationResult> PublishModelAsync(IList list, MachineLearningPublicationViewOption viewOption = MachineLearningPublicationViewOption.NewViewAsDefault);

        /// <summary>
        /// Publish this model to a library
        /// </summary>
        /// <param name="list">List to publish this model to</param>
        /// <param name="viewOption">Determines how adding the model changes the library's views</param>
        /// <returns>Information about the model publication</returns>
        public ISyntexModelPublicationResult PublishModel(IList list, MachineLearningPublicationViewOption viewOption = MachineLearningPublicationViewOption.NewViewAsDefault);

        /// <summary>
        /// Publish this model to a library
        /// </summary>
        /// <param name="targetLibraryServerRelativeUrl">Server relative url of the library to publish the model to</param>
        /// <param name="targetSiteUrl">Full url (https://...) of the site collection hosting the library to publish the model to</param>
        /// <param name="targetWebServerRelativeUrl">Server relative url of the web hosting the library to publish the model to</param>
        /// <param name="viewOption">Determines how adding the model changes the library's views</param>
        /// <returns>Information about the model publication</returns>
        public Task<ISyntexModelPublicationResult> PublishModelAsync(string targetLibraryServerRelativeUrl, string targetSiteUrl, string targetWebServerRelativeUrl, MachineLearningPublicationViewOption viewOption = MachineLearningPublicationViewOption.NewViewAsDefault);

        /// <summary>
        /// Publish this model to a library
        /// </summary>
        /// <param name="targetLibraryServerRelativeUrl">Server relative url of the library to publish the model to</param>
        /// <param name="targetSiteUrl">Full url (https://...) of the site collection hosting the library to publish the model to</param>
        /// <param name="targetWebServerRelativeUrl">Server relative url of the web hosting the library to publish the model to</param>
        /// <param name="viewOption">Determines how adding the model changes the library's views</param>
        /// <returns>Information about the model publication</returns>
        public ISyntexModelPublicationResult PublishModel(string targetLibraryServerRelativeUrl, string targetSiteUrl, string targetWebServerRelativeUrl, MachineLearningPublicationViewOption viewOption = MachineLearningPublicationViewOption.NewViewAsDefault);

        /// <summary>
        /// Unpublish this model from the library
        /// </summary>
        /// <param name="list">List to publish this model to</param>
        /// <returns>Information about the model unpublication</returns>
        public Task<ISyntexModelPublicationResult> UnPublishModelAsync(IList list);

        /// <summary>
        /// Unpublish this model from the library
        /// </summary>
        /// <param name="list">List to publish this model to</param>
        /// <returns>Information about the model unpublication</returns>
        public ISyntexModelPublicationResult UnPublishModel(IList list);

        /// <summary>
        /// Unpublish this model from the library
        /// </summary>
        /// <param name="targetLibraryServerRelativeUrl">Server relative url of the library to publish the model to</param>
        /// <param name="targetSiteUrl">Full url (https://...) of the site collection hosting the library to publish the model to</param>
        /// <param name="targetWebServerRelativeUrl">Server relative url of the web hosting the library to publish the model to</param>
        /// <returns>Information about the model unpublication</returns>
        public Task<ISyntexModelPublicationResult> UnPublishModelAsync(string targetLibraryServerRelativeUrl, string targetSiteUrl, string targetWebServerRelativeUrl);

        /// <summary>
        /// Unpublish this model from the library
        /// </summary>
        /// <param name="targetLibraryServerRelativeUrl">Server relative url of the library to publish the model to</param>
        /// <param name="targetSiteUrl">Full url (https://...) of the site collection hosting the library to publish the model to</param>
        /// <param name="targetWebServerRelativeUrl">Server relative url of the web hosting the library to publish the model to</param>
        /// <returns>Information about the model unpublication</returns>
        public ISyntexModelPublicationResult UnPublishModel(string targetLibraryServerRelativeUrl, string targetSiteUrl, string targetWebServerRelativeUrl);
        #endregion
    }
}
