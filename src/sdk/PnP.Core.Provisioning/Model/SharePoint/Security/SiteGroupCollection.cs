namespace PnP.Core.Provisioning.Model
{
    /// <summary>
    /// Collection of SiteGroup objects
    /// </summary>
    public partial class SiteGroupCollection : BaseProvisioningTemplateObjectCollection<SiteGroup>
    {
        /// <summary>
        /// Constructor for SiteGroupCollection class
        /// </summary>
        /// <param name="parentTemplate">Parent provisioning template</param>
        public SiteGroupCollection(ProvisioningTemplate parentTemplate) : base(parentTemplate)
        {

        }
    }
}
