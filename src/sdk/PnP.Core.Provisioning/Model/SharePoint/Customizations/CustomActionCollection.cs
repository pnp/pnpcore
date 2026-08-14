namespace PnP.Core.Provisioning.Model
{
    /// <summary>
    /// Collection of CustomAction objects
    /// </summary>
    public partial class CustomActionCollection : BaseProvisioningTemplateObjectCollection<CustomAction>
    {
        /// <summary>
        /// Constructor for CustomActionCollection class
        /// </summary>
        /// <param name="parentTemplate">Parent provisioning template</param>
        public CustomActionCollection(ProvisioningTemplate parentTemplate) : base(parentTemplate)
        {

        }
    }
}
