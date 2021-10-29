namespace PnP.Core.Admin.Model.SharePoint
{
    /// <summary>
    /// Contains the values of the 3 allowed states for Special Characters
    /// </summary>
    public enum SpecialCharactersState
    {
        /// <summary>
        /// Setting was not set
        /// </summary>
        NoPreference = 0,

        /// <summary>
        /// Special characters like #,% are allowed in the names of files and folders
        /// </summary>
        Allowed = 1,

        /// <summary>
        /// Special characters like #,% are not allowed in the names of files and folders
        /// </summary>
        Disallowed = 2
    }
}
