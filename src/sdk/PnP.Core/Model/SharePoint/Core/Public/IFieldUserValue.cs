using PnP.Core.Model.Security;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Represents a user retrieved via a list item field
    /// </summary>
    public interface IFieldUserValue : IFieldLookupValue
    {
        /// <summary>
        /// Principal describing the user or group
        /// </summary>
        public ISharePointPrincipal Principal { get; set; }
        
        /// <summary>
        /// Email of the user
        /// </summary>
        public string Email { get; }

        /// <summary>
        /// Title/name of the user
        /// </summary>
        public string Title { get; }

        /// <summary>
        /// SIP address of the user
        /// </summary>
        public string Sip { get; }

        /// <summary>
        /// Profile picture url for the user
        /// </summary>
        public string Picture { get; }
    }
}
