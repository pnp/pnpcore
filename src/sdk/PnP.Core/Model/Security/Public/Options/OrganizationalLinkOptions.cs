namespace PnP.Core.Model.Security
{
    /// <summary>
    /// Properties that can be set when creating a new Organizational Link
    /// </summary>
    public class OrganizationalLinkOptions 
    {

        /// <summary>
        /// The type of sharing link to create.
        /// </summary>
        public ShareType Type { get; set; }
    }
}
