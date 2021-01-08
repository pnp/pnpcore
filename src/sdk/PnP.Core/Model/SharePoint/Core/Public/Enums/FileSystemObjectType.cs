namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Specifies the file system object type.
    /// </summary>
    public enum FileSystemObjectType
    {
        /// <summary>
        /// Enumeration whose values specify whether the object is invalid. The value = -1.
        /// </summary>
        Invalid = -1,

        /// <summary>
        /// Enumeration whose values specify whether the object is a file. The value = 0.
        /// </summary>
        File = 0,

        /// <summary>
        /// Enumeration whose values specify whether the object is a folder. The value = 1.
        /// </summary>
        Folder = 1,

        /// <summary>
        /// Enumeration whose values specify whether the object is a site. The values = 2.
        /// </summary>
        Web = 2
    }
}
