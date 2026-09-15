namespace PnP.Core.Provisioning.Model
{
    /// <summary>
    /// Collection of RoleAssignment objects
    /// </summary>
    public partial class RoleAssignmentCollection : BaseProvisioningTemplateObjectCollection<RoleAssignment>
    {
        /// <summary>
        /// Constructor for RoleAssignmentCollection class
        /// </summary>
        /// <param name="parentTemplate">Parent provisioning template</param>
        public RoleAssignmentCollection(ProvisioningTemplate parentTemplate) : base(parentTemplate)
        {

        }
    }
}
