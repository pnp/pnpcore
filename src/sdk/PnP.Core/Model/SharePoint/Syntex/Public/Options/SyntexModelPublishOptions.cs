namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Information about the library to publish a Syntex model to
    /// </summary>
    public class SyntexModelPublishOptions : SyntexModelUnPublishOptions
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public SyntexModelPublishOptions()
        {
            ViewOption = MachineLearningPublicationViewOption.NewViewAsDefault;
        }

        /// <summary>
        /// The view option specified when registering the model with the library
        /// </summary>
        public MachineLearningPublicationViewOption ViewOption { get; set; }
    }
}
