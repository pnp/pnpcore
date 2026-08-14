namespace PnP.Core.Provisioning.Model
{
    /// <summary>
    /// Collection of WebPart objects
    /// </summary>
    public partial class WebPartCollection : BaseProvisioningTemplateObjectCollection<WebPart>
    {
        /// <summary>
        /// Constructor for WebPartCollection class
        /// </summary>
        /// <param name="parentTemplate">Parent provisioning template</param>
        public WebPartCollection(ProvisioningTemplate parentTemplate) : base(parentTemplate)
        {

        }
    }
}
