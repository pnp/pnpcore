namespace PnP.Core.Model
{
    /// <summary>
    /// Attribute to define how a model property maps to a SharePoint field for usage in SharePoint REST calls
    /// </summary>
    internal class SharePointPropertyAttribute : PropertyAttribute
    {
        internal SharePointPropertyAttribute(string fieldName)
        {
            FieldName = fieldName;
        }

    }
}
