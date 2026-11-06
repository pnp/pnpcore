namespace PnP.Core.Provisioning.Model.Teams
{
    public partial class TeamSecurityUserCollection : BaseProvisioningTemplateObjectCollection<TeamSecurityUser>
    {
        /// <summary>
        /// Constructor for TeamSecurityUserCollection class
        /// </summary>
        /// <param name="parentTemplate">Parent provisioning template</param>
        public TeamSecurityUserCollection(ProvisioningTemplate parentTemplate) :
            base(parentTemplate)
        {
        }
    }
}
