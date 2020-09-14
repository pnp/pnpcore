namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Specifies the type of the Recycle Bin item.
    /// </summary>
    public enum RecycleBinItemType
    {
        /// <summary>
        /// The type of the Recycle Bin item is not specified. The value = 0.
        /// </summary>
        None = 0,
        /// <summary>
        /// Specifies that the Recycle Bin item is a file. The value = 1.
        /// </summary>
        File = 1,
        /// <summary>
        /// Specifies that the Recycle Bin item is a historical version of a file. The value = 2.
        /// </summary>
        FileVersion = 2,
        /// <summary>
        /// Specifies that the Recycle Bin item is a list item. The value = 3.
        /// </summary>
        ListItem = 3,
        /// <summary>
        /// Specifies that the Recycle Bin item is a list. The value = 4.
        /// </summary>
        List = 4,
        /// <summary>
        /// Specifies that the Recycle Bin item is a folder. The value = 5.
        /// </summary>
        Folder = 5,
        /// <summary>
        /// Specifies that the Recycle Bin item is a folder that contains a list. The value = 6.
        /// </summary>
        FolderWithLists = 6,
        /// <summary>
        /// Specifies that the Recycle Bin item is an attachment. The value = 7.
        /// </summary>
        Attachment = 7,
        /// <summary>
        /// Specifies that the Recycle Bin item is a historical version of a list item. The value = 8.
        /// </summary>
        ListItemVersion = 8,
        /// <summary>
        /// Specifies that the Recycle Bin item is a list item that is the parent of one or more related list items. The value = 9.
        /// </summary>
        CascadeParent = 9,
        /// <summary>
        /// Specifies that the Recycle Bin item is a site (Web object). The value = 10.
        /// </summary>
        Web = 10,
        /// <summary>
        /// Specifies that that Recycle Bin item is an app. The value = 11
        /// </summary>
        App = 11,
    }
}
