namespace PnP.Core.Provisioning.Model.Drive
{
    /// <summary>
    /// Collection of DriveFile items
    /// </summary>
    public partial class DriveFileCollection : BaseProvisioningTemplateObjectCollection<DriveFile>
    {
        /// <summary>
        /// Constructor for DriveFileCollection class
        /// </summary>
        /// <param name="parentTemplate">Parent provisioning template</param>
        public DriveFileCollection(ProvisioningTemplate parentTemplate) :
            base(parentTemplate)
        {
        }
    }
}
