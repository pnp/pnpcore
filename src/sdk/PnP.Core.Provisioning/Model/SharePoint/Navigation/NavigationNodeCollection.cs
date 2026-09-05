namespace PnP.Core.Provisioning.Model
{
    /// <summary>
    /// A collection of NavigationNode objects
    /// </summary>
    public partial class NavigationNodeCollection : BaseProvisioningTemplateObjectCollection<NavigationNode>
    {
        /// <summary>
        /// Constructor for NavigationNodeCollection class
        /// </summary>
        /// <param name="parentTemplate">Parent provisioning template</param>
        public NavigationNodeCollection(ProvisioningTemplate parentTemplate) :
            base(parentTemplate)
        {
        }
    }
}
