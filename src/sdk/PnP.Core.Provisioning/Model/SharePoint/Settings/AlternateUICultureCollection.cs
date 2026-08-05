namespace PnP.Core.Provisioning.Model
{
    /// <summary>
    /// Collection of AlternateUICulture objects
    /// </summary>
    public partial class AlternateUICultureCollection : BaseProvisioningTemplateObjectCollection<AlternateUICulture>
    {
        /// <summary>
        /// Constructor for AlternateUICultureCollection class
        /// </summary>
        /// <param name="parentTemplate">Parent provisioning template</param>
        public AlternateUICultureCollection(ProvisioningTemplate parentTemplate) :
            base(parentTemplate)
        {
        }
    }
}
