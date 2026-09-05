namespace PnP.Core.Provisioning.Model
{
    /// <summary>
    /// Collection of View objects
    /// </summary>
    public partial class ViewCollection : BaseProvisioningTemplateObjectCollection<View>
    {
        /// <summary>
        /// Constructor for ViewCollection class
        /// </summary>
        /// <param name="parentTemplate">Parent provisioning template</param>
        public ViewCollection(ProvisioningTemplate parentTemplate) : base(parentTemplate)
        {

        }
    }
}
