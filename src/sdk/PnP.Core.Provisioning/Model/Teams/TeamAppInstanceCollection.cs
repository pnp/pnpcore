namespace PnP.Core.Provisioning.Model.Teams
{
    /// <summary>
    /// Defines the Apps to install or update on the Team
    /// </summary>
    public partial class TeamAppInstanceCollection : BaseProvisioningTemplateObjectCollection<TeamAppInstance>
    {
        /// <summary>
        /// Constructor for TeamAppInstanceCollection class
        /// </summary>
        /// <param name="parentTemplate">Parent provisioning template</param>
        public TeamAppInstanceCollection(ProvisioningTemplate parentTemplate) :
            base(parentTemplate)
        {
        }
    }
}
