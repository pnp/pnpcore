namespace PnP.Core.Provisioning.Model
{
    /// <summary>
    /// The Webhooks for the Provisioning Template
    /// </summary>
    public partial class ProvisioningWebhookCollection : BaseProvisioningTemplateObjectCollection<ProvisioningWebhook>
    {
        public ProvisioningWebhookCollection(ProvisioningTemplate parentTemplate) : base(parentTemplate)
        {

        }
    }
}
