namespace PnP.Core.Provisioning.Model
{
    /// <summary>
    /// Collection of User objects
    /// </summary>
    public partial class UserCollection : BaseProvisioningTemplateObjectCollection<User>
    {
        /// <summary>
        /// Constructor for UserCollection class
        /// </summary>
        /// <param name="parentTemplate">Parent provisioning template</param>
        public UserCollection(ProvisioningTemplate parentTemplate) : base(parentTemplate)
        {

        }
    }
}
