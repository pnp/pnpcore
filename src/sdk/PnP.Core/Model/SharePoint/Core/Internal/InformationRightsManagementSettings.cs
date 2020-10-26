using PnP.Core.Services;
using System.Net.Http;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// InformationRightsManagementSettings class, write your custom code here
    /// </summary>
    [SharePointType("SP.InformationRightsManagementSettings", Target = typeof(InformationRightsManagementSettings), Uri = "_api/web/lists/getbyid(guid'{Parent.Id}')/InformationRightsManagementSettings")]
    internal partial class InformationRightsManagementSettings
    {
        public InformationRightsManagementSettings()
        {
        }
    }
}
