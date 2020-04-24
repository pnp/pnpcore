namespace PnP.Core.Model
{
    /// <summary>
    /// Public interface to define an object extensible through a dictionary of metadata properties
    /// </summary>
    public interface IExpandoDataModel
    {
        /// <summary>
        /// The dictionary of metadata properties
        /// </summary>
        TransientDictionary Values { get; }

        /// <summary>
        /// Transforms the current model instance into a dynamic type
        /// </summary>
        /// <returns>Dynamic version of current model instance</returns>
        dynamic ToDynamic();
    }
}
