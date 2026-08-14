namespace PnP.Core.Provisioning.Model
{
    /// <summary>
    /// Collection of TermSete objects
    /// </summary>
    public partial class TermSetCollection : BaseProvisioningTemplateObjectCollection<TermSet>
    {
        /// <summary>
        /// Constructor for TermSetCollection class
        /// </summary>
        /// <param name="parentTemplate">Parent provisioning template</param>
        public TermSetCollection(ProvisioningTemplate parentTemplate) : base(parentTemplate)
        {

        }
    }
}
