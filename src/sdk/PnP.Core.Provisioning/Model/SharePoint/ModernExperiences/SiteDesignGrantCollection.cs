namespace PnP.Core.Provisioning.Model
{
    /// <summary>
    /// Collection of SiteDesignGrant objects
    /// </summary>
    public partial class SiteDesignGrantCollection : BaseProvisioningTemplateObjectCollection<SiteDesignGrant>
    {
        /// <summary>
        /// Constructor for SiteDesignGrantCollection
        /// </summary>
        /// <param name="parentTemplate">Parent provisioning template</param>
        public SiteDesignGrantCollection(ProvisioningTemplate parentTemplate) : base(parentTemplate)
        {
        }
    }
}
