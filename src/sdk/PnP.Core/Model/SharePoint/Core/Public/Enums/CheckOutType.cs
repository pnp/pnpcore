namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Enumeration that describes the different checkout states of a file, independent of the lock state of the file.
    /// (e.g. https://docs.microsoft.com/en-us/previous-versions/office/sharepoint-server/ee538918(v=office.15))
    /// </summary>
    public enum CheckOutType
    {
        /// <summary>
        ///   The file is checked out for editing on the server.
        /// </summary>
        Online,
        /// <summary>
        /// The file is checked out for editing on the local computer.
        /// </summary>
        Offline,
        /// <summary>
        /// The file is not checked out.
        /// </summary>
        None
    }
}
