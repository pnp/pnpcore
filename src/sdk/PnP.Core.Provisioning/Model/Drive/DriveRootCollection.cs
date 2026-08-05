namespace PnP.Core.Provisioning.Model.Drive
{
    /// <summary>
    /// Collection of DriveRoot items
    /// </summary>
    public partial class DriveRootCollection : BaseProvisioningTemplateObjectCollection<DriveRoot>
    {
        /// <summary>
        /// Constructor for DriveRootCollection class
        /// </summary>
        /// <param name="parentTemplate">Parent provisioning template</param>
        public DriveRootCollection(ProvisioningTemplate parentTemplate) :
            base(parentTemplate)
        {
        }
    }
}
