namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Available options for all SharePoint fields
    /// </summary>
    public abstract class FieldOptions
    {
        /// <summary>
        /// Sets whether the field is required
        /// </summary>
        public bool? Required { get; set; }
    }

    #region NOT (YET) SUPPORTED
    // NOTE Is it relevant to implement ?
    // Looks like REST API doesn't work to add those type of fields
    /// <summary>
    /// Available options for SharePoint computed fields
    /// </summary>
    //public class FieldComputedOptions : FieldOptions
    //{
    //    /// <summary>
    //    /// Gets or sets a value that specifies whether a lookup field can reference the field.
    //    /// </summary>
    //    public bool EnableLookup { get; set; }
    //}

    ///// <summary>
    ///// Available options for SharePoint Rating Scale fields
    ///// </summary>
    //public class FieldRatingScale : FieldMultiChoiceOptions
    //{
    //    /// <summary>
    //    /// Gets or sets a value that specifies the end number for the rating scale.
    //    /// </summary>
    //    public int GridEndNumber { get; set; }

    //    /// <summary>
    //    /// Gets or sets a value that specifies the display text corresponding to the choice in the rating scale that indicates the non-applicable option.
    //    /// </summary>
    //    public string GridNAOptionText { get; set; }

    //    /// <summary>
    //    /// Gets or sets a value that specifies the start number for the rating scale.
    //    /// </summary>
    //    public int GridStartNumber { get; set; }

    //    /// <summary>
    //    /// Gets or sets a value that specifies the display text corresponding to the average of the rating scale.
    //    /// </summary>
    //    public string GridTextRangeAverage { get; set; }

    //    /// <summary>
    //    /// Gets or sets a value that specifies the display text corresponding to the maximum of the rating scale.
    //    /// </summary>
    //    public string GridTextRangeHigh { get; set; }

    //    /// <summary>
    //    /// Gets or sets a value that specifies the display text corresponding to the minimum of the rating scale.
    //    /// </summary>
    //    public string GridTextRangeLow { get; set; }

    //    /// <summary>
    //    /// Gets pr sets the range count of the field.
    //    /// </summary>
    //    public int RangeCount { get; set; }
    //}
    #endregion
}
