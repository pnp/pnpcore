namespace PnP.Core.Provisioning.Model.Teams
{
    /// <summary>
    /// Collection of Apps for Microsoft Teams
    /// </summary>
    public partial class TeamAppCollection : BaseProvisioningTemplateObjectCollection<TeamApp>
    {
        /// <summary>
        /// Constructor for TeamAppCollection class
        /// </summary>
        /// <param name="parentTemplate">Parent provisioning template</param>
        public TeamAppCollection(ProvisioningTemplate parentTemplate) :
            base(parentTemplate)
        {
        }
    }
}
