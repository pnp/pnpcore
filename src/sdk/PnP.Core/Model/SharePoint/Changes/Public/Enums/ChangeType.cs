namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Enumeration of the possible types of changes.
    /// </summary>
    public enum ChangeType
    {
        /// <summary>
        /// Enumeration whose values indicate that no change has taken place.
        /// </summary>
        NoChange = 0,

        /// <summary>
        /// Enumeration whose values specify that an object has been added within the scope of a list, site, site collection, or content database.
        /// </summary>
        Add = 1,

        /// <summary>
        /// Enumeration whose values specify that an object has been modified within the scope of a list, site, site collection, or content database.
        /// </summary>
        Update = 2,

        /// <summary>
        /// Enumeration whose values specify that an object has been deleted within the scope of a list, site, site collection, or content database.
        /// </summary>
        DeleteObject = 3,

        /// <summary>
        /// Enumeration whose values specify that the leaf in a URL has been renamed.
        /// </summary>
        Rename = 4,

        /// <summary>
        /// Enumeration whose values specify that a non-leaf section within a URL has been renamed. The object was moved away from the location within the site specified by the change.
        /// </summary>
        MoveAway = 5,

        /// <summary>
        /// Enumeration whose values specify that a non-leaf section within a URL has been renamed. The object was moved into the location within the site specified by the change.
        /// </summary>
        MoveInto = 6,

        /// <summary>
        /// Enumeration whose values specify that an object has restored from a backup or from the recycle bin.
        /// </summary>
        Restore = 7,

        /// <summary>
        /// Enumeration whose values specify that a role definition has been added.
        /// </summary>
        RoleAdd = 8,

        /// <summary>
        /// Enumeration whose values specify that a role definition has been deleted.
        /// </summary>
        RoleDelete = 9,

        /// <summary>
        /// Enumeration whose values specify that a role definition has been updated.
        /// </summary>
        RoleUpdate = 10,

        /// <summary>
        /// <para>Enumeration whose values specify that a user has been given permissions to a list.</para>
        /// <para>The list must have unique permissions enabled.</para>
        /// </summary>
        AssignmentAdd = 11,

        /// <summary>
        /// <para>Enumeration whose values specify that a user has lost permissions to a list.</para>
        /// <para>The list must have unique permissions enabled.</para>
        /// </summary>
        AssignmentDelete = 12,

        /// <summary>
        /// Enumeration whose values specify that a user has been added to a group.
        /// </summary>
        MemberAdd = 13,

        /// <summary>
        /// Enumeration whose values specify that a user has been removed from a group.
        /// </summary>
        MemberDelete = 14,

        /// <summary>
        /// Enumeration whose values specify that a change has been made to an item using the SystemUpdate method.
        /// </summary>
        SystemUpdate = 15,

        /// <summary>
        /// Enumeration whose values specify that a change in the navigation structure of a site collection has been made.
        /// </summary>
        Navigation = 16,

        /// <summary>
        /// Enumeration whose values specify that a change in permissions scope has been made to break inheritance from an object's parent.
        /// </summary>
        ScopeAdd = 17,

        /// <summary>
        /// Enumeration whose values specify that a change in permissions scope has been made to revert back to inheriting permissions from an object's parent.
        /// </summary>
        ScopeDelete = 18,

        /// <remarks>
        /// Undefined
        /// </remarks>
        ListContentTypeAdd,

        /// <remarks>
        /// Undefined
        /// </remarks>
        ListContentTypeDelete,

        /// <remarks>
        /// Undefined
        /// </remarks>
        Dirty,

        /// <remarks>
        /// Undefined
        /// </remarks>
        Activity
    }
}