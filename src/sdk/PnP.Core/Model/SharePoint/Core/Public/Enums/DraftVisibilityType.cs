namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Specifies the kind of user who can view the minor version of a document draft.
    /// </summary>
    public enum DraftVisibilityType
    {
        /// <summary>
        /// Readers can see minor versions of the document.
        /// </summary>
        Reader = 0,

        /// <summary>
        /// Authors and approvers can see minor versions of the document.
        /// </summary>
        Author = 1,

        /// <summary>
        /// Approvers can see minor versions of the document.
        /// </summary>
        Approver = 2
    }
}
