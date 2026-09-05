namespace PnP.Core.Provisioning.Model
{
    /// <summary>
    /// Collection of ProvisioningTemplate items
    /// </summary>
    public partial class ProvisioningTemplateCollection : BaseProvisioningHierarchyObjectCollection<ProvisioningTemplate>
    {
        /// <summary>
        /// Constructor for ProvisioningTemplateCollection class
        /// </summary>
        /// <param name="parentProvisioning">Parent Provisioning object</param>
        public ProvisioningTemplateCollection(ProvisioningHierarchy parentProvisioning) :
            base(parentProvisioning)
        {
        }
    }
}
