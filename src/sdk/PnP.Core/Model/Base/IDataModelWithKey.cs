namespace PnP.Core.Model
{
    /// <summary>
    /// Defines the basic behavior for a Domain Model object with an identifying key
    /// </summary>
    public interface IDataModelWithKey
    {
        /// <summary>
        /// Readonly property to get the untyped key of the object
        /// </summary>
        object Key { get; set; }
    }
}
