namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// The result of a SharePoint Syntex model publication
    /// </summary>
    public interface ISyntexModelPublicationResult
    {
        /// <summary>
        /// The publication error (if there was any)
        /// </summary>
        string ErrorMessage { get; }

        /// <summary>
        /// The published model
        /// </summary>
        ISyntexModelPublication Publication { get; }

        /// <summary>
        /// The status code of the publication, 2xx == success
        /// </summary>
        int StatusCode { get; }

        /// <summary>
        /// Did the publication succeed?
        /// </summary>
        bool Succeeded { get; }
    }
}
