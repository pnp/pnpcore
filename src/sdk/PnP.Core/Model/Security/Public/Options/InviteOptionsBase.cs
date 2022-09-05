namespace PnP.Core.Model.Security
{
    /// <summary>
    /// Base class for InviteOptions classes
    /// </summary>
    public abstract class InviteOptionsBase
    {
        /// <summary>
        /// Creates a new IDriveRecipient for the passed email address
        /// </summary>
        /// <param name="email">Email of the user to create an <see cref="IDriveRecipient"/> for</param>
        /// <returns>An <see cref="IDriveRecipient"/> instance</returns>
        public static IDriveRecipient CreateDriveRecipient(string email)
        {
            return new DriveRecipient
            {
                Email = email
            };
        }
    }
}
