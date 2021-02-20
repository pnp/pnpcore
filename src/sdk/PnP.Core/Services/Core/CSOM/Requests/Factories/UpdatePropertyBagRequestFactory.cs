using PnP.Core.Model;
using PnP.Core.Model.SharePoint;
using PnP.Core.Services.Core.CSOM.Requests.ListItems;
using PnP.Core.Services.Core.CSOM.Requests.Web;
using System;
using System.Collections.Generic;
using System.Text;

namespace PnP.Core.Services.Core.CSOM.Requests.Factories
{
    class UpdatePropertyBagRequestFactory
    {
        public static UpdatePropertyBagRequest GetUpdatePropertyBagRequest<T>(T propertyValueContainer) where T : IDataModelWithContext
        {
            UpdatePropertyBagRequest result = new UpdatePropertyBagRequest();
            if ((propertyValueContainer as IDataModelParent).Parent is IFolder)
            {
                if (((propertyValueContainer as IDataModelParent).Parent as IFolder).IsPropertyAvailable(p => p.ServerRelativeUrl))
                {
                    result = new FolderPropertyBagUpdateRequest(((propertyValueContainer as IDataModelParent).Parent as IFolder).ServerRelativeUrl);
                }
                else
                {
                    throw new ClientException(ErrorType.Unsupported, PnPCoreResources.Exception_Unsupported_FileServerRelativeUrlNotLoaded);
                }
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
