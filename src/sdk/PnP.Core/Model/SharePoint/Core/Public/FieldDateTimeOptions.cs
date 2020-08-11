namespace PnP.Core.Model.SharePoint.Core.Public
{
    /// <summary>
    /// Available options for SharePoint DateTime fields
    /// </summary>
    public class FieldDateTimeOptions : CommonFieldOptions
    {
        /// <summary>
        /// Gets or sets a value that specifies the calendar type of the field.
        /// </summary>
        public CalendarType DateTimeCalendarType { get; set; }

        /// <summary>
        /// Gets or sets the type of date and time format that is used in the field.
        /// </summary>
        public DateTimeFieldFormatType DisplayFormat { get; set; }

        /// <summary>
        /// Gets or sets the type of friendly display format that is used in the field.
        /// </summary>
        public DateTimeFieldFriendlyFormatType FriendlyDisplayFormat { get; set; }
    }
}
