namespace PnP.Core.Provisioning.Model
{
    /// <summary>
    /// Collection of TermLabel objects
    /// </summary>
    public partial class TermLabelCollection : BaseProvisioningTemplateObjectCollection<TermLabel>
    {
        /// <summary>
        /// Constructor for TermLabelCollection class
        /// </summary>
        /// <param name="parentTemplate">Parent provisioning template</param>
        public TermLabelCollection(ProvisioningTemplate parentTemplate) : base(parentTemplate)
        {

        }
    }
}
