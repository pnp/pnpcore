namespace PnP.Core.Provisioning.Model.AzureActiveDirectory
{
    /// <summary>
    /// Collection of AAD Users
    /// </summary>
    public partial class UserCollection : BaseProvisioningTemplateObjectCollection<User>
    {
        /// <summary>
        /// Constructor for UserCollection class
        /// </summary>
        /// <param name="parentTemplate">Parent provisioning template</param>
        public UserCollection(ProvisioningTemplate parentTemplate) :
            base(parentTemplate)
        {
        }
    }
}
