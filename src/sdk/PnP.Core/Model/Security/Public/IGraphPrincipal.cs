namespace PnP.Core.Model.Security
{
    /// <summary>
    /// Represents a Microsoft 365 user or group 
    /// </summary>
    public interface IGraphPrincipal
    {
        /// <summary>
        /// Gets a value that specifies the member identifier for the user or group.
        /// </summary>
        public string Id { get; set; }
    }
}
