namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Key/Value term set property
    /// </summary>
    public interface ITermSetProperty : IComplexType
    {
        /// <summary>
        /// Property key
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// Property value
        /// </summary>
        public string Value { get; set; }

    }
}
