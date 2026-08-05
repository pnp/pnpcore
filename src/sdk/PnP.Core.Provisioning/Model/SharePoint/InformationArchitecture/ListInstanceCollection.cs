namespace PnP.Core.Provisioning.Model
{
    /// <summary>
    /// Collection of ListInstance objects
    /// </summary>
    public partial class ListInstanceCollection : BaseProvisioningTemplateObjectCollection<ListInstance>
    {
        /// <summary>
        /// Constructor for ListInstanceCollection class
        /// </summary>
        /// <param name="parentTemplate">Parent provisioning template</param>
        public ListInstanceCollection(ProvisioningTemplate parentTemplate) : base(parentTemplate)
        {

        }
    }
}
