namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Available options for SharePoint Number fields
    /// </summary>
    public class FieldNumberOptions : CommonFieldOptions
    {
        /*
        // (Not an enum in CSOM FieldNumber class : https://docs.microsoft.com/en-us/previous-versions/office/sharepoint-csom/mt827700%28v%3doffice.15%29 )
        
        /// <summary>
        /// Gets or sets the display format of the field.
        /// Althought in the docs as usable parameters, DisplayFormat is always -1 in response for number fields
        /// </summary>
        //public int DisplayFormat { get; set; }
        */

        /// <summary>
        /// Gets or sets a value that specifies the maximum allowed value for the field.
        /// </summary>
        public double? MaximumValue { get; set; }

        /// <summary>
        /// Gets or sets a value that specifies the minimum allowed value for the field.
        /// </summary>
        public double? MinimumValue { get; set; }

        /// <summary>
        /// Gets or sets whether the field must be shown as percentage.
        /// </summary>
        public bool? ShowAsPercentage { get; set; }
    }
}
