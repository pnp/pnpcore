namespace PnP.Core.Provisioning.Model
{
    /// <summary>
    /// Collection of Footer Links for the target site
    /// </summary>
    public partial class SiteFooterLinkCollection : BaseProvisioningTemplateObjectCollection<SiteFooterLink>
    {
        /// <summary>
        /// Constructor for SiteFooterLinkCollection class
        /// </summary>
        /// <param name="parentTemplate">Parent provisioning template</param>
        public SiteFooterLinkCollection(ProvisioningTemplate parentTemplate) :
            base(parentTemplate)
        {
        }
    }
}
