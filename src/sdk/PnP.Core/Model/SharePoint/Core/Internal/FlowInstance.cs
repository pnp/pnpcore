namespace PnP.Core.Model.SharePoint
{
    internal sealed class FlowInstance : IFlowInstance
    {
        public string Id { get; set; }

        public string DisplayName { get; set; }

        public string Definition { get; set; }
    }
}
