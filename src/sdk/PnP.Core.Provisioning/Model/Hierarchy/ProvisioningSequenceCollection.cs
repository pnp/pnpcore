namespace PnP.Core.Provisioning.Model
{
    /// <summary>
    /// Collection of ProvisioningSequence items
    /// </summary>
    public partial class ProvisioningSequenceCollection : BaseProvisioningHierarchyObjectCollection<ProvisioningSequence>
    {
        /// <summary>
        /// Constructor for ProvisioningSequenceCollection class
        /// </summary>
        /// <param name="parentProvisioning">Parent Provisioning object</param>
        public ProvisioningSequenceCollection(ProvisioningHierarchy parentProvisioning) :
            base(parentProvisioning)
        {
        }
    }
}
