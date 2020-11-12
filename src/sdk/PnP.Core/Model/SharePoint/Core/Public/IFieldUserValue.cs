namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Represents a user retrieved via a list item field
    /// </summary>
    public interface IFieldUserValue : IFieldValue
    {
        /// <summary>
        /// Email of the user
        /// </summary>
        public string Email { get; }

        /// <summary>
        /// Id in the SharePoint site collection for the user
        /// </summary>
        public int Id { get; set; }

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
