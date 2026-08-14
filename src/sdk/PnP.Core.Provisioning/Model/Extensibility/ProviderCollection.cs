namespace PnP.Core.Provisioning.Model
{
    /// <summary>
    /// Collection of Provider objects
    /// </summary>
    public partial class ProviderCollection : BaseProvisioningTemplateObjectCollection<Provider>
    {
        /// <summary>
        /// Constructor for ProviderCollection class
        /// </summary>
        /// <param name="parentTemplate">Parent provisioning template</param>
        public ProviderCollection(ProvisioningTemplate parentTemplate) : base(parentTemplate)
        {

        }
    }
}
