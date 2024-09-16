namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Specifies the base type for a list.
    /// </summary>
    public enum ListBaseType : int
    {
        /// <summary>
        /// No base type is specified.
        /// </summary>
        None = -1,

        /// <summary>
        /// Specifies a base type for lists that do not correspond to another base type in this enumeration.
        /// </summary>
        GenericList,            // 0

        /// <summary>
        /// Specifies a base type for document libraries.
        /// </summary>
        DocumentLibrary,        // 1

        /// <summary>
        /// Reserved.  MUST not be used.
        /// </summary>
        Unused,                 // 2

        /// <summary>
        /// Deprecated - used for discussion board lists pre-WSS V3.
        /// Newer discussion boards use the GenericList base type and ListTemplateType.DiscussionBoard.
        /// </summary>
        DiscussionBoard,        // 3

        /// <summary>
        /// Specifies a base type for survey lists.
        /// </summary>
        Survey,                 // 4

        /// <summary>
        /// Specifies a base type for issue tracking lists.
        /// </summary>
        Issue,                  // 5
    }
}
