namespace PnP.Core.Provisioning.Model
{
    /// <summary>
    /// The Webhooks for the Provisioning Template
    /// </summary>
    public partial class ProvisioningTemplateWebhookCollection : BaseProvisioningTemplateObjectCollection<ProvisioningTemplateWebhook>
    {
        public ProvisioningTemplateWebhookCollection(ProvisioningTemplate parentTemplate) : base(parentTemplate)
        {

        }
    }
}
