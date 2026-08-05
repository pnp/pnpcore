namespace PnP.Core.Provisioning.Model
{
    /// <summary>
    /// A collection of AddIn objects
    /// </summary>
    public partial class AddInCollection : BaseProvisioningTemplateObjectCollection<AddIn>
    {
        /// <summary>
        /// Constructor for AddInCollection class
        /// </summary>
        /// <param name="parentTemplate">Parent provisioning template</param>
        public AddInCollection(ProvisioningTemplate parentTemplate) :
            base(parentTemplate)
        {
        }
    }
}
