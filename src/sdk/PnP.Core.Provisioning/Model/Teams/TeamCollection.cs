namespace PnP.Core.Provisioning.Model.Teams
{
    /// <summary>
    /// Collection of Teams for Microsoft Teams
    /// </summary>
    public partial class TeamCollection : BaseProvisioningTemplateObjectCollection<Team>
    {
        /// <summary>
        /// Constructor for TeamCollection class
        /// </summary>
        /// <param name="parentTemplate">Parent provisioning template</param>
        public TeamCollection(ProvisioningTemplate parentTemplate) :
            base(parentTemplate)
        {
        }
    }
}
