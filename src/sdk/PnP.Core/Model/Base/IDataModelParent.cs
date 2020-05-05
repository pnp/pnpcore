namespace PnP.Core.Model
{
    /// <summary>
    /// Interface to implement parent concept on all model objects
    /// </summary>
    public interface IDataModelParent
    {
        /// <summary>
        /// Represents the parent of the current domain model object
        /// </summary>
        IDataModelParent Parent { get; set; }
    }
}