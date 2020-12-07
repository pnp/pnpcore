namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Available options for SharePoint Choice fields
    /// </summary>
    public class FieldChoiceOptions : FieldChoiceMultiOptions
    {
        /// <summary>
        /// Determines whether to display the choice field as option buttons (also known as “radio buttons”) or as a drop-down list.
        /// </summary>
        public ChoiceFormatType EditFormat { get; set; }
    }
}
