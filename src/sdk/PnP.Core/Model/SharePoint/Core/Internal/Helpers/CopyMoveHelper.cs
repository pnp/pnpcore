using PnP.Core.Services;
using System.Net;

namespace PnP.Core.Model.SharePoint
{
    internal class CopyMoveHelper
    {
        internal static ApiCall GetCopyToApiCall<TModel>(BaseDataModel<TModel> entity, string destinationServerRelativeUrl, bool overwrite)
        {
            var entityInfo = EntityManager.Instance.GetClassInfo<TModel>(typeof(TModel), entity);
            // NOTE WebUtility encode spaces to "+" instead of %20
            string encodedDestinationUrl = WebUtility.UrlEncode(destinationServerRelativeUrl).Replace("+", "%20").Replace("/", "%2F");
            string copyToEndpointUrl = $"{entityInfo.SharePointUri}/copyTo(strnewurl='{encodedDestinationUrl}', boverwrite={overwrite.ToString().ToLower()})";

            return  new ApiCall(copyToEndpointUrl, ApiType.SPORest);
        }


        internal static ApiCall GetMoveToApiCall<TModel>(BaseDataModel<TModel> entity, string destinationServerRelativeUrl, MoveOperations moveOperations)
        {
            var entityInfo = EntityManager.Instance.GetClassInfo<TModel>(typeof(TModel), entity);
            // NOTE WebUtility encode spaces to "+" instead of %20
            string encodedDestinationUrl = WebUtility.UrlEncode(destinationServerRelativeUrl).Replace("+", "%20").Replace("/", "%2F");
            string moveToEndpointUrl = $"{entityInfo.SharePointUri}/moveTo(newurl='{encodedDestinationUrl}', flags={(int)moveOperations})";

            return new ApiCall(moveToEndpointUrl, ApiType.SPORest);
        }
    }
}
