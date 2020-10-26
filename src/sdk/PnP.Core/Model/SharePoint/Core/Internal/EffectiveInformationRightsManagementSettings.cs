using Microsoft.Extensions.Logging;
using PnP.Core.Services;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// EffectiveInformationRightsManagementSettings class, write your custom code here
    /// </summary>
    [SharePointType("SP.EffectiveInformationRightsManagementSettings", Target = typeof(IFile))]
    internal partial class EffectiveInformationRightsManagementSettings
    {
        public EffectiveInformationRightsManagementSettings()
        {
        }
    }
}
