namespace PnP.Core.Transformation.SharePoint.Publishing
{
    /// <summary>
    /// Class that will be used to hold the fields that will be used the field to metadata mapping
    /// </summary>
    internal class PageLayoutMetadataEntity
    {
        internal string FieldName { get; set; }
        internal string TargetFieldName { get; set; }
        internal string Functions { get; set; }
    }
}
