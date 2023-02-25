using System.Collections.Generic;

namespace PnP.Core.Admin.Model.SharePoint
{
    /// <summary>
    /// Properties of the SharePoint Service Principal
    /// </summary>
    public interface IServicePrincipalProperties
    {
        /// <summary>
        /// Service principal enabled
        /// </summary>
        bool AccountEnabled { get; set; }
        
        /// <summary>
        /// Client/App Id of the principal
        /// </summary>
        string AppId { get; set; }

        /// <summary>
        /// Allowed reply urls 
        /// </summary>
        IEnumerable<string> ReplyUrls { get; set; }
    }
}