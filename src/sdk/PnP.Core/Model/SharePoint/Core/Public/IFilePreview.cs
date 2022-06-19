namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Response object from a Graph Preview call on an IFile object
    /// Either getUrl, postUrl, or both might be returned depending on the current state of embed support for the specified options.
    /// postParameters is a string formatted as application/x-www-form-urlencoded, and if performing a POST to the postUrl the content-type should be set accordingly. For example:
    /// POST https://www.onedrive.com/embed_by_post
    /// Content-Type: application/x-www-form-urlencoded
    /// param1=value&amp;param2=another%20value
    /// </summary>
    public interface IFilePreview
    {
        /// <summary>
        /// URL suitable for embedding using HTTP GET (iframes, etc.)
        /// </summary>
        string GetUrl { get; }

        /// <summary>
        /// URL suitable for embedding using HTTP POST (form post, JS, etc.)
        /// </summary>
        string PostUrl { get; }

        /// <summary>
        /// POST parameters to include if using postUrl
        /// </summary>
        string PostParameters { get; }
    }
}
