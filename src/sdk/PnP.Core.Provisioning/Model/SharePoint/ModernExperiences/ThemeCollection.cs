namespace PnP.Core.Provisioning.Model
{
    /// <summary>
    /// A collection of Theme objects
    /// </summary>
    public partial class ThemeCollection : BaseProvisioningTemplateObjectCollection<Theme>
    {
        /// <summary>
        /// Constructor for ThemeCollection class
        /// </summary>
        /// <param name="parentTemplate">Parent provisioning template</param>
        public ThemeCollection(ProvisioningTemplate parentTemplate) :
            base(parentTemplate)
        {
        }
    }
}
