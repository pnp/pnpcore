namespace PnP.Core.Provisioning.Model
{
    /// <summary>
    /// Collection of SiteCollection items
    /// </summary>
    public partial class SiteCollectionCollection : BaseProvisioningHierarchyObjectCollection<SiteCollection>
    {
        /// <summary>
        /// Constructor for SiteCollectionCollection class
        /// </summary>
        /// <param name="parentHierarchy">Parent Provisioning object</param>
        public SiteCollectionCollection(ProvisioningHierarchy parentHierarchy) :
            base(parentHierarchy)
        {
        }
    }
}
