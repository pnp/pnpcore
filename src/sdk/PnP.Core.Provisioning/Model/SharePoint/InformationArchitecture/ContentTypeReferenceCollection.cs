namespace PnP.Core.Provisioning.Model
{
    /// <summary>
    /// Collection of Content Type References objects
    /// </summary>
    public partial class ContentTypeReferenceCollection : BaseProvisioningTemplateObjectCollection<ContentTypeReference>
    {
        /// <summary>
        /// Constructor for ContentTypeReferenceCollection class
        /// </summary>
        /// <param name="parentTemplate">Parent provisioning template</param>
        public ContentTypeReferenceCollection(ProvisioningTemplate parentTemplate) : base(parentTemplate)
        {

        }
    }
}
