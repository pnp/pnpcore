using System.Threading.Tasks;

namespace PnP.Core.Model.SharePoint
{
    [SharePointType("SP.Web", Uri = "_api/web", LinqGet = "_api/web/webinfos")]
    [GraphType(Get = "sites/{hostname}:{serverrelativepath}")]
    internal class SyntexContentCenter : Web, ISyntexContentCenter
    {
        public Task GetSyntexModelsAsync(string modelName = null)
        {
            throw new System.NotImplementedException();
        }
    }
}
