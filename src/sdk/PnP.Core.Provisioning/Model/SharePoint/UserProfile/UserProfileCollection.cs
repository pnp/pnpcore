namespace PnP.Core.Provisioning.Model.SPUPS
{
    /// <summary>
    /// Collection of UserProfile items
    /// </summary>
    public partial class UserProfileCollection : BaseProvisioningTemplateObjectCollection<UserProfile>
    {
        /// <summary>
        /// Constructor for UserProfileCollection class
        /// </summary>
        /// <param name="parentTemplate">Parent provisioning template</param>
        public UserProfileCollection(ProvisioningTemplate parentTemplate) :
            base(parentTemplate)
        {
        }
    }
}
