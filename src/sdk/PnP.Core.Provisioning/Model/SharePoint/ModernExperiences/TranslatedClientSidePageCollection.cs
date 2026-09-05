namespace PnP.Core.Provisioning.Model
{
    /// <summary>
    /// A collection of TranslatedClientSidePage objects
    /// </summary>
    public partial class TranslatedClientSidePageCollection : BaseProvisioningTemplateObjectCollection<TranslatedClientSidePage>
    {
        /// <summary>
        /// Constructor for TranslatedClientSidePageCollection class
        /// </summary>
        /// <param name="parentTemplate">Parent provisioning template</param>
        public TranslatedClientSidePageCollection(ProvisioningTemplate parentTemplate) :
            base(parentTemplate)
        {
        }
    }
}
