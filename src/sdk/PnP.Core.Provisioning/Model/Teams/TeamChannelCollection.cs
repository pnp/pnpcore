namespace PnP.Core.Provisioning.Model.Teams
{
    /// <summary>
    /// Defines a collection of Channels for the Team
    /// </summary>
    public partial class TeamChannelCollection : BaseProvisioningTemplateObjectCollection<TeamChannel>
    {
        /// <summary>
        /// Constructor for TeamChannelCollection class
        /// </summary>
        /// <param name="parentTemplate">Parent provisioning template</param>
        public TeamChannelCollection(ProvisioningTemplate parentTemplate) :
            base(parentTemplate)
        {
        }
    }
}
