namespace PnP.Core.Model
{
    /// <summary>
    /// Defines the very basic interface for every Domain Model object.
    /// Add methods are implemented in their respective model interfaces
    /// </summary>
    /// <typeparam name="TModel">The actual type of the Domain Model object</typeparam>
    public interface IDataModel<TModel> : IDataModelParent, IDataModelWithContext, IDataModelGet<TModel>
    {
        /// <summary>
        /// Checks if a property on this model object has a value set
        /// </summary>
        /// <param name="propertyName">Property to check</param>
        /// <returns>True if set, false otherwise</returns>
        bool HasValue(string propertyName = "");

        /// <summary>
        /// Checks if a property on this model object has changed
        /// </summary>
        /// <param name="propertyName">Property to check</param>
        /// <returns>True if changed, false otherwise</returns>
        bool HasChanged(string propertyName = "");

        /// <summary>
        /// Was this model requested from the back-end
        /// </summary>
        bool Requested { get; set; }
    }
}
