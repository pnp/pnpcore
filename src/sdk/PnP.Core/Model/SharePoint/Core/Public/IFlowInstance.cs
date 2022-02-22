namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Describes a flow instance linked to a library/list
    /// </summary>
    public interface IFlowInstance
    {
        /// <summary>
        /// Unique id defining the flow instance
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// Display name of the flow instance
        /// </summary>
        public string DisplayName { get; }

        /// <summary>
        /// JSON definition of the flow instance
        /// </summary>
        public string Definition { get; }
    }
}
