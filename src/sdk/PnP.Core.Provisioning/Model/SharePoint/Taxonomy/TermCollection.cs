namespace PnP.Core.Provisioning.Model
{
    /// <summary>
    /// Collection of Term objects
    /// </summary>
    public partial class TermCollection : BaseProvisioningTemplateObjectCollection<Term>
    {
        /// <summary>
        /// Constructor for TermCollection class
        /// </summary>
        /// <param name="parentTemplate">Parent provisioning template</param>
        public TermCollection(ProvisioningTemplate parentTemplate) : base(parentTemplate)
        {

        }
    }
}
