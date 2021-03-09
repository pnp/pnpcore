using PnP.Core.Model;
using PnP.Core.Model.SharePoint;
using PnP.Core.Services.Core.CSOM.Requests.ListItems;
using PnP.Core.Services.Core.CSOM.Requests.Web;

namespace PnP.Core.Services.Core.CSOM.Requests.Factories
{
    internal class UpdatePropertyBagRequestFactory
    {
        internal static UpdatePropertyBagRequest GetUpdatePropertyBagRequest<T>(T propertyValueContainer) where T : IDataModelWithContext
        {
            UpdatePropertyBagRequest result = new UpdatePropertyBagRequest();
            if ((propertyValueContainer as IDataModelParent).Parent is IFolder)
            {
                result = new FolderPropertyBagUpdateRequest(((propertyValueContainer as IDataModelParent).Parent as IFolder).UniqueId.ToString());
            }
            else if ((propertyValueContainer as IDataModelParent).Parent is IFile)
            {
                if (((propertyValueContainer as IDataModelParent).Parent as IFile).IsPropertyAvailable(p => p.ServerRelativeUrl))
                {
                    result = new FilePropertyBagUpdateRequest(((propertyValueContainer as IDataModelParent).Parent as IFile).ServerRelativeUrl);
                }
                else
                {
                    throw new ClientException(ErrorType.Unsupported, PnPCoreResources.Exception_Unsupported_FileServerRelativeUrlNotLoaded);
                }
            }

            result.SiteId = propertyValueContainer.PnPContext.Site.Id.ToString();
            result.WebId = propertyValueContainer.PnPContext.Web.Id.ToString();

            return result;
        }
    }
}
