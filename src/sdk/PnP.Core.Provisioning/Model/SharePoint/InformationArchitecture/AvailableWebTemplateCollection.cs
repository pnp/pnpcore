namespace PnP.Core.Provisioning.Model
{
    /// <summary>
    /// A collection of AvailableWebTemplate objects
    /// </summary>
    public partial class AvailableWebTemplateCollection : BaseProvisioningTemplateObjectCollection<AvailableWebTemplate>
    {
        /// <summary>
        /// Constructor for AvailableWebTemplateCollection class
        /// </summary>
        /// <param name="parentTemplate">Parent provisioning template</param>
        public AvailableWebTemplateCollection(ProvisioningTemplate parentTemplate) : base(parentTemplate)
        {

        }
    }
}
