namespace PnP.Core.Model
{
    /// <summary>
    /// Public interface to define an object extensible through a dictionary of metadata properties
    /// </summary>
    public interface IExpandoComplexType : IComplexType
    {
        /// <summary>
        /// Gets or sets dynamic properties
        /// </summary>
        /// <param name="key">The key of the property to get or set</param>
        /// <returns>The value of the property</returns>
        object this[string key] { get; set;  }

        /// <summary>
        /// Gets the count of properties of the current expando complex type object
        /// </summary>
        public int Count { get; }

        /// <summary>
        /// Transforms the current model instance into a dynamic type
        /// </summary>
        /// <returns>Dynamic version of current model instance</returns>
        dynamic AsDynamic();
    }
}
