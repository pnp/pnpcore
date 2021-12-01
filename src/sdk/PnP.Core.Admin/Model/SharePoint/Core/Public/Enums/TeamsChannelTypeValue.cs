namespace PnP.Core.Admin.Model.SharePoint
{
    /// <summary>
    /// Indicates the type of TeamsCannel a site is connected to
    /// </summary>
    public enum TeamsChannelTypeValue
    {
        /// <summary>
        /// This site is not connected to a teams channel.
        /// </summary>
        None = 0,
        
        /// <summary>
        /// This site is connected to a Teams private channel.
        /// </summary>
        PrivateChannel = 1,
        
        /// <summary>
        /// This site is connected to a Teams shared channel.
        /// </summary>
        SharedChannel = 2,

        /// <summary>
        /// This site is connected to a Teams standard channel.
        /// </summary>
        StandardChannel = 3
    }
}
