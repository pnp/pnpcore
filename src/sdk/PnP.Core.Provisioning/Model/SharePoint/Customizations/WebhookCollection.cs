namespace PnP.Core.Provisioning.Model
{
    /// <summary>
    /// Defines a collection of objects of type Webhook
    /// </summary>
    public partial class WebhookCollection : BaseProvisioningTemplateObjectCollection<Webhook>
    {
        /// <summary>
        /// Constructor for WebhookCollection class
        /// </summary>
        /// <param name="parentTemplate">Parent provisioning template</param>
        public WebhookCollection(ProvisioningTemplate parentTemplate) :
            base(parentTemplate)
        {
        }
    }
}
