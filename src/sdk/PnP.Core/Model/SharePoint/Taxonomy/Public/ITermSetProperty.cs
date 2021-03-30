namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Key/Value term set property
    /// </summary>
    [ConcreteType(typeof(TermSetProperty))]
    public interface ITermSetProperty : IDataModel<ITermSetProperty>
    {
        /// <summary>
        /// Property key
        /// </summary>
        public string KeyField { get; set; }

        /// <summary>
        /// Property value
        /// </summary>
        public string Value { get; set; }

    }
}