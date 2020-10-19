namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Key/Value term property
    /// </summary>
    public interface ITermProperty : IComplexType<ITermProperty>
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
