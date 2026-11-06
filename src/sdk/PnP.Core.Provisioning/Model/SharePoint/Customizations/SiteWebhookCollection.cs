namespace PnP.Core.Provisioning.Model
{
    /// <summary>
    /// Collection of SiteWebhook objects
    /// </summary>
    public partial class SiteWebhookCollection : BaseProvisioningTemplateObjectCollection<SiteWebhook>
    {
        /// <summary>
        /// Constructor for SiteWebhookCollection class
        /// </summary>
        /// <param name="parentTemplate">Parent provisioning template</param>
        public SiteWebhookCollection(ProvisioningTemplate parentTemplate) : base(parentTemplate)
        {

        }
    }
}
