using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Specifies the control settings while adding a field.
    /// This enumeration has a FlagsAttribute attribute that allows a bitwise combination of its member values.
    /// </summary>
    [Flags]
    public enum AddFieldOptionsFlags
    {
        /// <summary>
        /// Enumeration whose values specify that a new field added to the list must also be added to the default content type in the site collection.The value = 0.
        /// </summary>
        DefaultValue = 0,
        
        /// <summary>
        /// Enumeration whose values specify that a new field added to the list must also be added to the default content type in the site collection.The value = 1.
        /// </summary>
        AddToDefaultContentType = 1,
        
        /// <summary>
        ///  Enumeration whose values specify that a new field must not be added to any other content type.The value = 2.
        /// </summary>
        AddToNoContentType = 2,
        
        /// <summary>
        /// Enumeration whose values specify that a new field that is added to the specified list must also be added to all content types in the site collection.The value = 4.
        /// </summary>
        AddToAllContentTypes = 4,
        
        /// <summary>
        ///  Enumeration whose values specify adding an internal field name hint for the purpose of avoiding possible database locking or field renaming operations.The value = 8.
        /// </summary>
        AddFieldInternalNameHint = 8,
        
        /// <summary>
        ///  Enumeration whose values specify that a new field that is added to the specified list must also be added to the default list view.The value = 16.
        /// </summary>
        AddFieldToDefaultView = 16,
        
        /// <summary>
        /// Enumeration whose values specify to confirm that no other field has the same display name. The value = 32.
        /// /// </summary>
        AddFieldCheckDisplayName = 32
    }
}
