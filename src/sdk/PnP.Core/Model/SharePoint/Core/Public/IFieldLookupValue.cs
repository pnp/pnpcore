namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Represents a lookup field value
    /// </summary>
    public interface IFieldLookupValue : IFieldValue
    {
        /// <summary>
        /// Id of the looked-up item
        /// </summary>
        public int LookupId { get; set; }

        /// <summary>
        /// Value of the key property of the looked-up item
        /// </summary>
        public string LookupValue { get; }

        /// <summary>
        /// Is the value a secret value?
        /// </summary>
        public bool IsSecretFieldValue { get; }

    }
}
