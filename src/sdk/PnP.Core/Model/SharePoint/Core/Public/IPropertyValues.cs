using System.Threading.Tasks;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a PropertyValues object
    /// </summary>
    [ConcreteType(typeof(PropertyValues))]
    public interface IPropertyValues : IDataModel<IPropertyValues>, IExpandoDataModel
    {
        /// <summary>
        /// Get string typed property bag value. If does not contain, returns given default value.
        /// </summary>
        /// <param name="key">Key of the property bag entry to return</param>
        /// <param name="defaultValue">Default value of the property bag</param>
        /// <returns>Value of the property bag entry as string</returns>        
        string GetString(string key, string defaultValue);

        /// <summary>
        /// Get int typed property bag value. If does not contain, returns given default value.
        /// </summary>
        /// <param name="key">Key of the property bag entry to return</param>
        /// <param name="defaultValue">Default value of the property bag</param>
        /// <returns>Value of the property bag entry as int</returns>        
        int GetInteger(string key, int defaultValue);

        /// <summary>
        /// Get boolean typed property bag value. If does not contain, returns given default value.
        /// </summary>
        /// <param name="key">Key of the property bag entry to return</param>
        /// <param name="defaultValue">Default value of the property bag</param>
        /// <returns>Value of the property bag entry as boolean</returns>        
        bool GetBoolean(string key, bool defaultValue);

        /// <summary>
        /// Update the property bag
        /// </summary>
        /// <returns></returns>
        Task UpdateAsync();

        /// <summary>
        /// Update the property bag
        /// </summary>
        void Update();
    }
}
