using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Available options for adding most of SharePoint fields
    /// (Excepted Lookup fields)
    /// </summary>
    public abstract class CommonFieldOptions
    {
        /// <summary>
        /// Guid of the field
        /// </summary>
        public Guid? Id { get; set; }

        /// <summary>
        /// Sets whether the field is required
        /// </summary>
        public bool? Required { get; set; }

        /// <summary>
        /// Add this field to the default view
        /// </summary>
        public bool AddToDefaultView { get; set; }

        /// <summary>
        /// An <see cref="AddFieldOptionsFlags"/> flag that specifies the field options to be applied during add
        /// </summary>
        public AddFieldOptionsFlags Options { get; set; }

        /// <summary>
        /// Sets the description of the field
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Set a specific internal name for the field
        /// </summary>
        public string InternalName { get; set; }

        /// <summary>
        /// Sets the default formula of the field
        /// </summary>
        public string DefaultFormula { get; set; }

        /// <summary>
        /// Sets whether the field enforces unique values
        /// </summary>
        public bool? EnforceUniqueValues { get; set; }

        /// <summary>
        /// Sets the group of the field
        /// </summary>
        public string Group { get; set; }

        /// <summary>
        /// Sets whether the field is hidden
        /// </summary>
        public bool? Hidden { get; set; }

        /// <summary>
        /// Sets whether the field is indexed
        /// </summary>
        public bool? Indexed { get; set; }

        /// <summary>
        /// Sets the validation formula of the field
        /// </summary>
        public string ValidationFormula { get; set; }

        /// <summary>
        /// Sets the validation message of the field
        /// </summary>
        public string ValidationMessage { get; set; }

        /// <summary>
        /// Show this field on the list's edit form
        /// </summary>
        public bool? ShowInEditForm { get; set; }

        /// <summary>
        /// Show this field on the list's view forms
        /// </summary>
        public bool? ShowInViewForms { get; set; }

        /// <summary>
        /// Show this field on the list's new form
        /// </summary>
        public bool? ShowInNewForm { get; set; }

        /// <summary>
        /// Allows you to set custom formatting JSON (https://docs.microsoft.com/en-us/sharepoint/dev/declarative-customization/column-formatting#supported-column-types) on the field
        /// </summary>
        public string CustomFormatter { get; set; }
    }
}
