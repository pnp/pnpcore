namespace PnP.Core.Provisioning.Model
{
    /// <summary>
    /// Collection of Localization objects
    /// </summary>
    public partial class LocalizationCollection : BaseProvisioningTemplateObjectCollection<Localization>
    {
        #region Properties

        /// <summary>
        /// The Default Locale ID for localization settings
        /// </summary>
        public int DefaultLCID { get; set; }        

        #endregion

        /// <summary>
        /// Constructor for LocalizationCollection class
        /// </summary>
        /// <param name="parentTemplate">Parent provisioning template</param>
        public LocalizationCollection(ProvisioningTemplate parentTemplate) :
            base(parentTemplate)
        {
        }
    }
}
