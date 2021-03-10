using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Defines a model publication
    /// </summary>
    public interface ISyntexModelPublication
    {
        /// <summary>
        /// Unique id of the SharePoint Syntex model
        /// </summary>
        Guid ModelUniqueId { get; }

        /// <summary>
        /// Server relative url of the library registered with the model
        /// </summary>
        string TargetLibraryServerRelativeUrl { get; }

        /// <summary>
        /// Fully qualified URL of the site collection holding the library registered with the model
        /// </summary>
        string TargetSiteUrl { get; }

        /// <summary>
        /// Server relative url of the web holding the library registered with the model
        /// </summary>
        string TargetWebServerRelativeUrl { get; }

        /// <summary>
        /// The view option specified when registering the model with the library
        /// </summary>
        MachineLearningPublicationViewOption ViewOption { get; }
    }
}
