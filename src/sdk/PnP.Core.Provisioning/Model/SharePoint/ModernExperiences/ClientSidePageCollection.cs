namespace PnP.Core.Provisioning.Model
{
    /// <summary>
    /// A collection of ClientSidePage objects
    /// </summary>
    public partial class ClientSidePageCollection : BaseProvisioningTemplateObjectCollection<ClientSidePage>
    {
        /// <summary>
        /// Constructor for ClientSidePageCollection class
        /// </summary>
        /// <param name="parentTemplate">Parent provisioning template</param>
        public ClientSidePageCollection(ProvisioningTemplate parentTemplate) :
            base(parentTemplate)
        {
        }
    }
}
