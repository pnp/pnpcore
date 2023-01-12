namespace PnP.Core.Model.SharePoint
{

    /// <summary>
    /// This enum class defines a set of abstract roles that a user can be assigned to share a securable object in a document library
    /// </summary>
    public enum Role
    {
        /// <summary>
        /// A user has no permission to share a securable object.
        /// </summary>
        None,

        /// <summary>
        /// a user can only read a securable object.
        /// </summary>
        View,

        /// <summary>
        /// a user can edit or read a securable object, but cannot delete the object.
        /// </summary>
        Edit,

        /// <summary>
        /// a user is an owner of a securable object, who can manage permissions, and edit, read or delete the object.
        /// </summary>
        Owner,

        /// <summary>
        /// a user can only read a shared object.
        /// </summary>
        LimitedView,

        /// <summary>
        /// a user can edit or read a shared object, but cannot delete the object.
        /// </summary>
        LimitedEdit,

        /// <summary>
        /// a user can read a shared object, plus comment in/on a shared object.
        /// </summary>
        Review,

        /// <summary>
        /// a user can view a shared object only in web viewer, cannot download the object.
        /// </summary>
        RestrictedView,

        /// <summary>
        /// a user can submit files to a file request folder. Can not view or edit folder contents.
        /// </summary>
        Submit,

        /// <summary>
        /// a user can edit or read a securable object, plus manage lists. 
        /// Includes all rights in the SharePoint standard Contributor role, plus ManageLists.
        /// </summary>
        ManageList,
    }
}
