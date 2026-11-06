namespace PnP.Core.Provisioning.Model
{
    /// <summary>
    /// Collection of SubSite items
    /// </summary>
    public partial class SubSiteCollection : BaseProvisioningHierarchyObjectCollection<SubSite>
    {
        /// <summary>
        /// Constructor for SubSiteCollection class
        /// </summary>
        /// <param name="parentProvisioning">Parent Provisioning object</param>
        public SubSiteCollection(ProvisioningHierarchy parentProvisioning) :
            base(parentProvisioning)
        {
        }
    }
}
