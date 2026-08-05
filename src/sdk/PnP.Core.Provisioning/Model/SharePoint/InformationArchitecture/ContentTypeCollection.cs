namespace PnP.Core.Provisioning.Model
{
    /// <summary>
    /// Collection of ContentType objects
    /// </summary>
    public partial class ContentTypeCollection : BaseProvisioningTemplateObjectCollection<ContentType>
    {
        /// <summary>
        /// Constructor for ContentTypeCollection class
        /// </summary>
        /// <param name="parentTemplate">Parent provisioning template</param>
        public ContentTypeCollection(ProvisioningTemplate parentTemplate) : base(parentTemplate)
        {

        }
    }
}
