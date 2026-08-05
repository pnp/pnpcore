namespace PnP.Core.Provisioning.Model
{
    /// <summary>
    /// Collection of ContentTypeBinding objects
    /// </summary>
    public partial class ContentTypeBindingCollection : BaseProvisioningTemplateObjectCollection<ContentTypeBinding>
    {
        /// <summary>
        /// Constructor for ContentTypeBindingCollection class
        /// </summary>
        /// <param name="parentTemplate">Parent provisioning template</param>
        public ContentTypeBindingCollection(ProvisioningTemplate parentTemplate) : base(parentTemplate)
        {

        }
    }
}
