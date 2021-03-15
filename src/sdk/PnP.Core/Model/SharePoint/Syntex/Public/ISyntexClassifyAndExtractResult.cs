using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Result of a classify and extract operation requested for a file
    /// </summary>
    public interface ISyntexClassifyAndExtractResult
    {
        /// <summary>
        /// Date of the classify and extract request creation
        /// </summary>
        DateTime Created { get; }

        /// <summary>
        /// Date of the classify and extract request delivery
        /// </summary>
        DateTime DeliverDate { get; }

        /// <summary>
        /// Id of the classify and extract request
        /// </summary>
        Guid Id { get; }

        /// <summary>
        /// Type of this Syntex machine learning work item
        /// </summary>
        Guid WorkItemType { get; }

        /// <summary>
        /// The classify and extract error (if there was any)
        /// </summary>
        string ErrorMessage { get; }

        /// <summary>
        /// The status code of the classify and extract request, 2xx == success
        /// </summary>
        int StatusCode { get; }

        /// <summary>
        /// Status of the classify and extract request
        /// </summary>
        string Status { get; }

        /// <summary>
        /// Server relative url of the file requested to be classified and extracted
        /// </summary>
        string TargetServerRelativeUrl { get; }

        /// <summary>
        /// Url of the site containing the file requested to be classified and extracted
        /// </summary>
        string TargetSiteUrl { get; }

        /// <summary>
        /// Server relative url of the web containing the file requested to be classified and extracted
        /// </summary>
        string TargetWebServerRelativeUrl { get; }
    }
}
