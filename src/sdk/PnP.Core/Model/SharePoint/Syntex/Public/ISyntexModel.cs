using System;
using System.Collections.Generic;
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

        #region Get model publications

        /// <summary>
        /// Gets a list of libraries to which this model is published
        /// </summary>
        /// <returns>The list of libraries to which this model is published</returns>
        public Task<List<ISyntexModelPublication>> GetModelPublicationsAsync();

        /// <summary>
        /// Gets a list of libraries to which this model is published
        /// </summary>
        /// <returns>The list of libraries to which this model is published</returns>
        public List<ISyntexModelPublication> GetModelPublications();

        #endregion

        #region Publish model
        /// <summary>
        /// Publish this model to a library
        /// </summary>
        /// <param name="library">Library to publish this model to</param>
        /// <param name="viewOption">Determines how adding the model changes the library's views</param>
        /// <returns>Information about the model publication</returns>
        public Task<ISyntexModelPublicationResult> PublishModelAsync(IList library, MachineLearningPublicationViewOption viewOption = MachineLearningPublicationViewOption.NewViewAsDefault);

        /// <summary>
        /// Publish this model to a library
        /// </summary>
        /// <param name="library">Library to publish this model to</param>
        /// <param name="viewOption">Determines how adding the model changes the library's views</param>
        /// <returns>Information about the model publication</returns>
        public ISyntexModelPublicationResult PublishModel(IList library, MachineLearningPublicationViewOption viewOption = MachineLearningPublicationViewOption.NewViewAsDefault);

        /// <summary>
        /// Publish this model to a list of libraries
        /// </summary>
        /// <param name="libraries">List of libraries to publish this model to</param>
        /// <param name="viewOption">Determines how adding the model changes the library's views</param>
        /// <returns>Information about the model publication</returns>
        public Task<List<ISyntexModelPublicationResult>> PublishModelAsync(List<IList> libraries, MachineLearningPublicationViewOption viewOption = MachineLearningPublicationViewOption.NewViewAsDefault);

        /// <summary>
        /// Publish this model to a list of libraries
        /// </summary>
        /// <param name="libraries">List of libraries to publish this model to</param>
        /// <param name="viewOption">Determines how adding the model changes the library's views</param>
        /// <returns>Information about the model publication</returns>
        public List<ISyntexModelPublicationResult> PublishModel(List<IList> libraries, MachineLearningPublicationViewOption viewOption = MachineLearningPublicationViewOption.NewViewAsDefault);

        /// <summary>
        /// Publish this model to a library
        /// </summary>
        /// <param name="publicationOptions">Information defining the model publication</param>
        /// <returns>Information about the model publication</returns>
        public Task<ISyntexModelPublicationResult> PublishModelAsync(SyntexModelPublicationOptions publicationOptions);

        /// <summary>
        /// Publish this model to a library
        /// </summary>
        /// <param name="publicationOptions">Information defining the model publication</param>
        /// <returns>Information about the model publication</returns>
        public ISyntexModelPublicationResult PublishModel(SyntexModelPublicationOptions publicationOptions);

        /// <summary>
        /// Publish this model to a list of libraries
        /// </summary>
        /// <param name="publicationOptions">Information defining the model publications</param>
        /// <returns>Information about the model publications</returns>
        public Task<List<ISyntexModelPublicationResult>> PublishModelAsync(List<SyntexModelPublicationOptions> publicationOptions);

        /// <summary>
        /// Publish this model to a list of libraries
        /// </summary>
        /// <param name="publicationOptions">Information defining the model publications</param>
        /// <returns>Information about the model publications</returns>
        public List<ISyntexModelPublicationResult> PublishModel(List<SyntexModelPublicationOptions> publicationOptions);

        #endregion

        #region Unpublish model

        /// <summary>
        /// Unpublish this model from the library
        /// </summary>
        /// <param name="library">Library to unpublish this model from</param>
        /// <returns>Information about the model unpublication</returns>
        public Task<ISyntexModelPublicationResult> UnPublishModelAsync(IList library);

        /// <summary>
        /// Unpublish this model from the library
        /// </summary>
        /// <param name="library">Library to unpublish this model from</param>
        /// <returns>Information about the model unpublication</returns>
        public ISyntexModelPublicationResult UnPublishModel(IList library);

        /// <summary>
        /// Unpublish this model from the list of libraries
        /// </summary>
        /// <param name="libraries">List of libraries to unpublish this model from</param>
        /// <returns>Information about the model unpublication</returns>
        public Task<List<ISyntexModelPublicationResult>> UnPublishModelAsync(List<IList> libraries);

        /// <summary>
        /// Unpublish this model from the list of libraries
        /// </summary>
        /// <param name="libraries">List of libraries to unpublish this model from</param>
        /// <returns>Information about the model unpublication</returns>
        public List<ISyntexModelPublicationResult> UnPublishModel(List<IList> libraries);

        /// <summary>
        /// Unpublish this model from the library
        /// </summary>
        /// <param name="unPublicationOptions">Information defining the model unpublication</param>
        /// <returns>Information about the model unpublication</returns>
        public Task<ISyntexModelPublicationResult> UnPublishModelAsync(SyntexModelUnPublicationOptions unPublicationOptions);

        /// <summary>
        /// Unpublish this model from the library
        /// </summary>
        /// <param name="unPublicationOptions">Information defining the model unpublication</param>
        /// <returns>Information about the model unpublication</returns>
        public ISyntexModelPublicationResult UnPublishModel(SyntexModelUnPublicationOptions unPublicationOptions);

        /// <summary>
        /// Unpublish this model from a list of libraries
        /// </summary>
        /// <param name="unPublicationOptions">Information defining the model unpublications</param>
        /// <returns>Information about the model unpublications</returns>
        public Task<List<ISyntexModelPublicationResult>> UnPublishModelAsync(List<SyntexModelUnPublicationOptions> unPublicationOptions);

        /// <summary>
        /// Unpublish this model from a list of libraries
        /// </summary>
        /// <param name="unPublicationOptions">Information defining the model unpublications</param>
        /// <returns>Information about the model unpublications</returns>
        public List<ISyntexModelPublicationResult> UnPublishModel(List<SyntexModelUnPublicationOptions> unPublicationOptions);

        #endregion

        #endregion
    }
}
