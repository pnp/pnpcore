namespace PnP.Core.Provisioning.Model.Office365Groups
{
    /// <summary>
    /// Collection of Office365GroupLifecyclePolicy items
    /// </summary>
    public partial class Office365GroupLifecyclePolicyCollection : BaseProvisioningTemplateObjectCollection<Office365GroupLifecyclePolicy>
    {
        /// <summary>
        /// Constructor for Office365GroupLifecyclePolicyCollection class
        /// </summary>
        /// <param name="parentTemplate">Parent provisioning template</param>
        public Office365GroupLifecyclePolicyCollection(ProvisioningTemplate parentTemplate) :
            base(parentTemplate)
        {
        }
    }
}
