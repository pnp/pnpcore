namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Options that define how adding a classifier will impact the library's views
    /// </summary>
    public enum MachineLearningPublicationViewOption
    {
        /// <summary>
        /// We will create a new view for your destination library. Your classifier will appear as a content type with a view, and your extractors will appear as site columns.
        /// </summary>
        NewView,

        /// <summary>
        /// We will create a new view for your destination library. Your classifier will appear as a content type with a view, and your extractors will appear as site columns. 
        /// This new view will also be the default view
        /// </summary>
        NewViewAsDefault,

        /// <summary>
        /// No view is being added
        /// </summary>
        NoNewView
    }
}
