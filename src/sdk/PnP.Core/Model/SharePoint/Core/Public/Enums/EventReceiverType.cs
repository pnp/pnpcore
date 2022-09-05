namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Specifies the type of event that is handled by the event receiver.
    /// </summary>
    public enum EventReceiverType
    {
        /// <summary>
        /// Indicates that an invalid event receiver type has been specified. 
        /// </summary>
        InvalidReceiver = -1,

        /// <summary>
        /// Event that occurs before an item has been added.
        /// </summary>
        ItemAdding = 1,

        /// <summary>
        /// Event that occurs before an item is updated.
        /// </summary>
        ItemUpdating = 2,

        /// <summary>
        /// Event that occurs before an item is deleted
        /// </summary>
        ItemDeleting = 3,

        /// <summary>
        /// Event that occurs before an item has been checked in.
        /// </summary>
        ItemCheckingIn = 4,

        /// <summary>
        /// Event that occurs before an item is checked out.
        /// </summary>
        ItemCheckingOut = 5,

        /// <summary>
        /// Event that occurs before an item is unchecked out.
        /// </summary>
        ItemUncheckingOut = 6,

        /// <summary>
        /// Event that occurs before an attachment has been added to an item.
        /// </summary>
        ItemAttachmentAdding = 7,

        /// <summary>
        /// Event that occurs before an attachment has been removed from the item.
        /// </summary>
        ItemAttachmentDeleting = 8,

        /// <summary>
        /// Event that occurs before a file is moved.
        /// </summary>
        ItemFileMoving = 9,

        /// <summary>
        /// Event that occurs before a document version is deleted.
        /// </summary>
        ItemVersionDeleting = 11,

        /// <summary>
        /// Event that occurs before a field is added to a list.
        /// </summary>
        FieldAdding = 101,

        /// <summary>
        /// Event that occurs before a field is updated.
        /// </summary>
        FieldUpdating = 102,

        /// <summary>
        /// Event that occurs before a field is removed from a list.
        /// </summary>
        FieldDeleting = 103,

        /// <summary>
        ///	Event that occurs before a list is created. 
        /// </summary>
        ListAdding = 104,

        /// <summary>
        /// Event that occurs before a list is deleted.
        /// </summary>
        ListDeleting = 105,

        /// <summary>
        /// Event that occurs before a site collection is deleted.
        /// </summary>
        SiteDeleting = 106,

        /// <summary>
        ///	Event that occurs before a site is deleted.  
        /// </summary>
        WebDeleting = 202,

        /// <summary>
        /// Event that occurs before a site URL has been changed.
        /// </summary>
        WebMoving = 203,

        /// <summary>
        /// Event that occurs before a new site is created.
        /// </summary>
        WebAdding = 204,

        /// <summary>
        /// Event that occurs before a security group is added.
        /// </summary>
        GroupAdding = 301,

        /// <summary>
        /// Event that occurs before a security group is updated.
        /// </summary>
        GroupUpdating = 302,

        /// <summary>
        /// Event that occurs before a security group is deleted.
        /// </summary>
        GroupDeleting = 303,

        /// <summary>
        /// Event that occurs before a user is added to a security group.
        /// </summary>
        GroupUserAdding = 304,

        /// <summary>
        ///	Event that occurs before a user is deleted from a security group. 
        /// </summary>
        GroupUserDeleting = 305,

        /// <summary>
        /// Event that occurs before a role definition is added.
        /// </summary>
        RoleDefinitionAdding = 306,

        /// <summary>
        /// Event that occurs before a role definition is updated.
        /// </summary>
        RoleDefinitionUpdating = 307,

        /// <summary>
        /// Event that occurs before a role definition is deleted. 
        /// </summary>
        RoleDefinitionDeleting = 308,

        /// <summary>
        /// Event that occurs before a role assignment is added.
        /// </summary>
        RoleAssignmentAdding = 309,

        /// <summary>
        /// Event that occurs before a role assignment is deleted.
        /// </summary>
        RoleAssignmentDeleting = 310,

        /// <summary>
        ///	Event that occurs before an inheritance is broken.  
        /// </summary>
        InheritanceBreaking = 311,

        /// <summary>
        /// Event that occurs before an inheritance is restored.
        /// </summary>
        InheritanceResetting = 312,

        /// <summary>
        ///	Event that occurs before a workflow starts running 
        /// </summary>
        WorkflowStarting = 501,

        /// <summary>
        /// Event that occurs after an item has been added.
        /// </summary>
        ItemAdded = 10001,

        /// <summary>
        ///	Event that occurs after an item has been updated. 
        /// </summary>
        ItemUpdated = 10002,

        /// <summary>
        /// Event that occurs after an item has been deleted.
        /// </summary>
        ItemDeleted = 10003,

        /// <summary>
        /// Event that occurs after an item has been checked in.
        /// </summary>
        ItemCheckedIn = 10004,

        /// <summary>
        /// Event that occurs after an item has been checked out.
        /// </summary>
        ItemCheckedOut = 10005,

        /// <summary>
        /// Event that occurs after an item has been unchecked out.
        /// </summary>
        ItemUncheckedOut = 10006,

        /// <summary>
        /// Event that occurs after an attachment has been added to the item.
        /// </summary>
        ItemAttachmentAdded = 10007,

        /// <summary>
        /// Event that occurs after an attachment has been removed from the item.
        /// </summary>
        ItemAttachmentDeleted = 10008,

        /// <summary>
        /// Event that occurs after a file has been moved.
        /// </summary>
        ItemFileMoved = 10009,

        /// <summary>
        /// Event that occurs after a file is transformed from one type to another.
        /// </summary>
        ItemFileConverted = 10010,

        /// <summary>
        /// Event that occurs after a document version is deleted.
        /// </summary>
        ItemVersionDeleted = 10011,

        /// <summary>
        /// Event that occurs after a field has been added. 
        /// </summary>
        FieldAdded = 10101,

        /// <summary>
        /// Event that occurs after a field has been updated.
        /// </summary>
        FieldUpdated = 10102,

        /// <summary>
        /// Event that occurs after a field has been removed.
        /// </summary>
        FieldDeleted = 10103,

        /// <summary>
        /// Event that occurs after a list has been created. 
        /// </summary>
        ListAdded = 10104,

        /// <summary>
        /// Event that occurs after a list has been deleted.
        /// </summary>
        ListDeleted = 10105,

        /// <summary>
        /// Event that occurs after a site collection has been deleted.
        /// </summary>
        SiteDeleted = 10201,

        /// <summary>
        ///	Event that occurs after a site has been deleted.
        /// </summary>
        WebDeleted = 10202,

        /// <summary>
        /// Event that occurs after a site URL has been changed.
        /// </summary>
        WebMoved = 10203,

        /// <summary>
        /// Event that occurs after a new site has been created, but before that new site is provisioned.
        /// </summary>
        WebProvisioned = 10204,

        /// <summary>
        /// Event that occurs happens after a security group is added.
        /// </summary>
        GroupAdded = 10301,

        /// <summary>
        /// Event that occurs after a security group is updated.
        /// </summary>
        GroupUpdated = 10302,

        /// <summary>
        /// Event that occurs after a security group is deleted.
        /// </summary>
        GroupDeleted = 10303,

        /// <summary>
        /// Event that occurs after a user is added to a security group.
        /// </summary>
        GroupUserAdded = 10304,

        /// <summary>
        /// Event that occurs after a user is deleted from a security group.
        /// </summary>
        GroupUserDeleted = 10305,

        /// <summary>
        ///	Event that occurs after a role definition is added.
        /// </summary>
        RoleDefinitionAdded = 10306,

        /// <summary>
        /// Event that occurs after a role definition is updated. 
        /// </summary>
        RoleDefinitionUpdated = 10307,

        /// <summary>
        /// Event that occurs after a role definition is deleted.
        /// </summary>
        RoleDefinitionDeleted = 10308,

        /// <summary>
        /// Event that occurs after a role assignment is added.
        /// </summary>
        RoleAssignmentAdded = 10309,

        /// <summary>
        /// Event that occurs after a role definition is deleted.
        /// </summary>
        RoleAssignmentDeleted = 10310,

        /// <summary>
        /// Event that occurs after an inheritance is broken.
        /// </summary>
        InheritanceBroken = 10311,

        /// <summary>
        /// Event that occurs after an inheritance is restored. 
        /// </summary>
        InheritanceReset = 10312,

        /// <summary>
        ///	Event that occurs after a workflow has started running.
        /// </summary>
        WorkflowStarted = 10501,

        /// <summary>
        /// Event that occurs after a workflow has been postponed.
        /// </summary>
        WorkflowPostponed = 10502,

        /// <summary>
        /// Event that occurs after a workflow has completed running.
        /// </summary>
        WorkflowCompleted = 10503,

        /// <summary>
        /// Event that occurs when an instance of an external content type has been added.
        /// </summary>
        EntityInstanceAdded = 10601,

        /// <summary>
        /// Event that occurs when an instance of an external content type has been updated.
        /// </summary>
        EntityInstanceUpdated = 10602,

        /// <summary>
        /// Event that occurs when an instance of an external content type has been deleted.
        /// </summary>
        EntityInstanceDeleted = 10603,

        /// <summary>
        /// Event that occurs after an app is installed.
        /// </summary>
        AppInstalled = 10701,

        /// <summary>
        /// Event that occurs after an app is upgraded.
        /// </summary>
        AppUpgraded = 10702,

        /// <summary>
        ///	Event that occurs before an app is uninstalled. 
        /// </summary>
        AppUninstalling = 10703,

        /// <summary>
        /// Event that occurs after a list receives an e-mail message.
        /// </summary>
        EmailReceived = 20000,

        /// <summary>
        /// Identifies workflow event receivers, and is therefore not a true event type.
        /// </summary>
        ContextEvent = 32766

    }
}
