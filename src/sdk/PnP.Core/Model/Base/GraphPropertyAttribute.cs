namespace PnP.Core.Model
{
    /// <summary>
    /// Attribute to define how a model property maps to a Microsoft Graph field for usage in Microsoft Graph API calls
    /// </summary>
    internal class GraphPropertyAttribute : PropertyAttribute
    {
        internal GraphPropertyAttribute(string fieldName)
        {
            FieldName = fieldName;
        }

        /// <summary>
        /// Url to get this field, needed in case the field cannot be loaded via an expand
        /// </summary>
        public string GraphGet { get; set; }
    }
}
