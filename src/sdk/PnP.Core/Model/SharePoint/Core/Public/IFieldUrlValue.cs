namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Represents the value in a field of type Url
    /// </summary>
    public interface IFieldUrlValue : IFieldValue
    {
        /// <summary>
        /// Url
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// Description of the Url
        /// </summary>
        public string Description { get; set; }
    }
}
