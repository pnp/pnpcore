namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Represents a field value object
    /// </summary>
    public interface IFieldValue
    {
        /// <summary>
        /// Field linked to this field value
        /// </summary>
        public IField Field { get; }
    }
}
