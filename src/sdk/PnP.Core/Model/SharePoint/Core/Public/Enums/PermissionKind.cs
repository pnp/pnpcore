namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Index to check if flag for a permission is set or not in SPBasePermissions enumeration
    /// </summary>
    public enum PermissionKind
    {
        /// <summary>
        /// Has no permissions on the Site. Not available through the user interface.
        /// </summary>
        EmptyMask = 0, //0x0000000000000000
        /// <summary>
        /// View items in lists, documents in document libraries, and Web discussion comments.
        /// </summary>
        ViewListItems = 1,
        /// <summary>
        /// Add items to lists, documents to document libraries, and Web discussion
        ///  comments.
        /// </summary>
        AddListItems = 2,
        /// <summary>
        /// Edit items in lists, edit documents in document libraries, edit Web discussion comments
        ///  in documents, and customize Web Part Pages in document libraries.
        /// </summary>
        EditListItems = 3, //0x0000000000000004,
        /// <summary>
        /// Delete items from a list, documents from a document library, and Web discussion
        ///  comments in documents.
        /// </summary>
        DeleteListItems = 4, //0x0000000000000008,
        /// <summary>
        /// Approve a minor version of a list item or document.
        /// </summary>
        ApproveItems = 5, //0x0000000000000010,
        /// <summary>
        /// View the source of documents with server-side file handlers.
        /// </summary>
        OpenItems = 6, //0x0000000000000020,
        /// <summary>
        /// View past versions of a list item or document.
        /// </summary>
        ViewVersions = 7, //0x0000000000000040,
        /// <summary>
        /// Delete past versions of a list item or document.
        /// </summary>
        DeleteVersions = 8, //0x0000000000000080,
        /// <summary>
        /// Discard or check in a document which is checked out to another user.
        /// </summary>
        CancelCheckout = 9, //0x0000000000000100,
        /// <summary>
        /// Create, change, and delete personal views of lists.
        /// </summary>
        ManagePersonalViews = 10, //0x0000000000000200,
        /// <summary>
        /// Create and delete lists, add or remove columns in a list, and add or remove public
        ///  views of a list.
        /// </summary>
        ManageLists = 12, //0x0000000000000800,
        /// <summary>
        /// View forms, views, and application pages, and enumerate lists.
        /// </summary>
        ViewFormPages = 13, //0x0000000000001000,
        /// <summary>
        /// Make content of a list or document library retrieveable for anonymous users through SharePoint search.
        /// The list permissions in the site do not change.
        /// </summary>
        AnonymousSearchAccessList = 14, //0x0000000000002000,
        /// <summary>
        /// Allow users to open a Site, list, or folder to access items inside that container.
        /// </summary>
        Open = 17, //0x0000000000010000,
        /// <summary>
        /// View pages in a Site.
        /// </summary>
        ViewPages = 18, //0x0000000000020000,
        /// <summary>
        /// Add, change, or delete HTML pages or Web Part Pages, and edit the Site using
        ///  a Windows SharePoint Services compatible editor.
        /// </summary>
        AddAndCustomizePages = 19, //0x0000000000040000,
        /// <summary>
        /// Apply a theme or borders to the entire Site.
        /// </summary>
        ApplyThemeAndBorder = 20, //0x0000000000080000,
        /// <summary>
        /// Apply a style sheet (.css file) to the Site.
        /// </summary>
        ApplyStyleSheets = 21, //0x0000000000100000,
        /// <summary>
        /// View reports on Site usage.
        /// </summary>
        ViewUsageData = 22, //0x0000000000200000,
        /// <summary>
        /// Create a Site using Self-Service Site Creation.
        /// </summary>
        CreateSSCSite = 23, //0x0000000000400000,
        /// <summary>
        /// Create subsites such as team sites, Meeting Workspace sites, and Document Workspace sites.
        /// </summary>
        ManageSubwebs = 24, //0x0000000000800000,
        /// <summary>
        /// Create a group of users that can be used anywhere within the site collection.
        /// </summary>
        CreateGroups = 25, //0x0000000001000000,
        /// <summary>
        /// Create and change permission levels on the Site and assign permissions to users
        /// and groups.
        /// </summary>
        ManagePermissions = 26, //0x0000000002000000,
        /// <summary>
        /// Enumerate files and folders in a Site using Microsoft Office SharePoint Designer
        /// and WebDAV interfaces.
        /// </summary>
        BrowseDirectories = 27, //0x0000000004000000,
        /// <summary>
        /// View information about users of the Site.
        /// </summary>
        BrowseUserInfo = 28, //0x0000000008000000,
        /// <summary>
        /// Add or remove personal Web Parts on a Web Part Page.
        /// </summary>
        AddDelPrivateWebParts = 29, //0x0000000010000000,
        /// <summary>
        /// Update Web Parts to display personalized information.
        /// </summary>
        UpdatePersonalWebParts = 30, //0x0000000020000000,
        /// <summary>
        /// Grant the ability to perform all administration tasks for the Site as well as
        ///  manage content, activate, deactivate, or edit properties of Site scoped Features
        ///  through the object model or through the user interface (UI). When granted on the
        ///  root Site of a Site Collection, activate, deactivate, or edit properties of
        ///  site collection scoped Features through the object model. To browse to the Site
        ///  Collection Features page and activate or deactivate Site Collection scoped Features
        ///  through the UI, you must be a Site Collection administrator.
        /// </summary>
        ManageWeb = 31, //0x0000000040000000,
        /// <summary>
        /// Content of lists and document libraries in the Web site will be retrieveable for anonymous users through
        /// SharePoint search if the list or document library has AnonymousSearchAccessList set.
        /// </summary>
        AnonymousSearchAccessWebLists = 32, //0x0000000080000000,
        /// <summary>
        /// Use features that launch client applications. Otherwise, users must work on documents
        ///  locally and upload changes.
        /// </summary>
        UseClientIntegration = 37, //0x0000001000000000,
        /// <summary>
        /// Use SOAP, WebDAV, or Microsoft Office SharePoint Designer interfaces to access
        ///  the Site.
        /// </summary>
        UseRemoteAPIs = 38, //0x0000002000000000,
        /// <summary>
        /// Manage alerts for all users of the Site.
        /// </summary>
        ManageAlerts = 39, //0x0000004000000000,
        /// <summary>
        /// Create e-mail alerts.
        /// </summary>
        CreateAlerts = 40, //0x0000008000000000,
        /// <summary>
        /// Allows a user to change his or her user information, such as adding a picture.
        /// </summary>
        EditMyUserInfo = 41, //0x0000010000000000,
        /// <summary>
        /// Enumerate permissions on Site, list, folder, document, or list item.
        /// </summary>
        EnumeratePermissions = 63, //0x4000000000000000,
        /// <summary>
        /// Has all permissions on the Site. Not available through the user interface.
        /// </summary>
        FullMask = 65,
    }
}
