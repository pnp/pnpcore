namespace PnP.Core.Model.SharePoint.Core.Public
{
    /// <summary>
    /// Available options for adding most of SharePoint fields
    /// (Excepted Lookup fields)
    /// </summary>
    public abstract class CommonFieldOptions : FieldOptions
    {
        /// <summary>
        /// Sets the description of the field
        /// </summary>
        public string Description { get; set; }

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
    }
}
