namespace PnP.Core.Admin.Model.SharePoint
{
    /// <summary>
    /// Contains the values of the 3 allowed options for Object Character Recognition
    /// </summary>
    public enum ObjectCharacterRecognitionMode
    {
        /// <summary>
        /// State 0: Disabled. OCR for the tenant is disabled. Default
        /// </summary>
        Disabled = 0,

        /// <summary>
        /// State 1: InclusionList. OCR for the tenant is filtered to a list of included items
        /// </summary>
        InclusionList,

        /// <summary>
        /// State 2: ExclusionList. OCR for the tenant is filtered to a list of excluded items
        /// </summary>
        ExclusionList
    }
}
