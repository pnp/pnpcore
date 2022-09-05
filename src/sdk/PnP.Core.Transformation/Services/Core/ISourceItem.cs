namespace PnP.Core.Transformation.Services.Core
{
    /// <summary>
    /// Represents the source item
    /// </summary>
    public interface ISourceItem
    {
        /// <summary>
        /// Gets the id of the source item
        /// </summary>
        ISourceItemId Id { get; }
    }
}
