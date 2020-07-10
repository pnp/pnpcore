using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Options to setup on the <see cref="RenderListDataOptions"/> RenderOptions property
    /// See https://docs.microsoft.com/en-us/sharepoint/dev/sp-add-ins/working-with-lists-and-list-items-with-rest#sprenderlistdataoptions-options
    /// </summary>
    [Flags]
    public enum RenderListDataOptionsFlags
    {
        /// <summary>
        /// Return default output
        /// </summary>
        None = 0,

        /// <summary>
        /// Return list context information
        /// </summary>
        ContextInfo = 1,

        /// <summary>
        /// Return list data (same as None)
        /// </summary>
        ListData = 2,

        /// <summary>
        /// Return list schema
        /// </summary>
        ListSchema = 4,

        /// <summary>
        /// Return HTML for the list menu
        /// </summary>
        MenuView = 8,

        /// <summary>
        /// Returns information about list content types. Must be combined with the ContextInfo flag
        /// </summary>
        ListContentType = 16,

        /// <summary>
        /// The returned list will have a FileSystemItemId field on each item if possible. Must be combined with the ListData flag
        /// </summary>
        FileSystemItemId = 32,

        /// <summary>
        /// Returns the client form schema to add and edit items
        /// </summary>
        ClientFormSchema = 64,

        /// <summary>
        /// Returns QuickLaunch navigation nodes
        /// </summary>
        QuickLaunch = 128,

        /// <summary>
        /// Returns Spotlight rendering information
        /// </summary>
        Spotlight = 256,

        /// <summary>
        /// Returns Visualization rendering information
        /// </summary>
        Visualization = 512,

        /// <summary>
        /// Returns view XML and other information about the current view
        /// </summary>
        ViewMetadata = 1024,

        /// <summary>
        /// Prevents AutoHyperlink from being run on text fields in this query
        /// </summary>
        DisableAutoHyperlink = 2048,

        /// <summary>
        /// Enables URLs pointing to Media TA service, such as .thumbnailUrl, .videoManifestUrl, .pdfConversionUrls
        /// </summary>
        EnableMediaTAUrls = 4096,

        /// <summary>
        /// Returns parent folder information
        /// </summary>
        ParentInfo = 8192,

        /// <summary>
        /// Returns page context info for the current list being rendered
        /// </summary>
        PageContextInfo = 16384,

        /// <summary>
        /// Return client-side component manifest information associated with the list (reserved for future use)
        /// </summary>
        ClientSideComponentManifest = 32768,
    }
}
