namespace PnP.Core.Model.Security
{
    /// <summary>
    /// Options containing the message and whether to save the mail to the sent items of the user or not.
    /// </summary>
    public class MailOptions
    {
        /// <summary>
        /// The message to send. Required.
        /// </summary>
        public MessageOptions Message { get; set; }

        /// <summary>
        /// Indicates whether to save the message in Sent Items. Specify it only if the parameter is false; default is true. Optional.
        /// </summary>
        public bool SaveToSentItems { get; set; } = true;

#if DEBUG
        /// <summary>
        /// The provisioning code will work differently if Application permissions are used. Defaults to live checking of the 
        /// current access token if not set.
        /// </summary>
        internal bool? UsingApplicationPermissions { get; set; }
#endif
    }
}
