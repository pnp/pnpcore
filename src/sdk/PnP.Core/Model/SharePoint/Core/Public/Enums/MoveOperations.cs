using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Specifies criteria for how to move files.
    /// This enumeration has a FlagsAttribute attribute that allows a bitwise combination of its member values.
    /// (see https://docs.microsoft.com/en-us/previous-versions/office/sharepoint-server/ee542328(v=office.15) )
    /// </summary>
    [Flags]
    public enum MoveOperations
    {
        /// <summary>
        /// No move operation specified. The value = 0.
        /// </summary>
        None = 0,
        
        /// <summary>
        /// Overwrite a file with the same name if it exists. The value = 1.
        /// </summary>
        Overwrite = 1,
        
        /// <summary>
        ///  Complete the move operation even if supporting files are separated from the file. The value = 8.
        /// </summary>
        AllowBrokenThickets = 8,

        /// <summary>
        /// Do not require approval permissions for move if there are no published versions for the document
        /// </summary>
        BypassApprovePermission = 64,

        /// <summary>
        /// Ignore Shared Lock constraints when moving this file. If one or more users have a valid Shared Lock,
        /// bypass the lock check during the move operation.
        /// </summary>
        BypassSharedLock = 256,

        /// <summary>
        /// Boolean specifying whether to retain the source of the move's editor and modified by datetime.
        /// </summary>
        RetainEditorAndModifiedOnMove = 2048
    }
}
