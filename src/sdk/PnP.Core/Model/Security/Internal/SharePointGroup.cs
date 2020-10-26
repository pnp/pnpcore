using Microsoft.Extensions.Logging;
using PnP.Core.Services;

namespace PnP.Core.Model.Security
{
    [SharePointType("SP.Group", Uri = "_api/Web/sitegroups/getbyid({id})", LinqGet = "_api/Web/SiteGroups")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2243:Attribute string literals should parse correctly", Justification = "<Pending>")]
    internal partial class SharePointGroup
    {
        public SharePointGroup()
        {
        }
    }
}
