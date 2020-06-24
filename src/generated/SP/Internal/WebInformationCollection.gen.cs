using PnP.Core.QueryModel.Model;
using PnP.Core.Services;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Collection of WebInformation Domain Model objects
    /// </summary>
    internal partial class WebInformationCollection : QueryableDataModelCollection<IWebInformation>, IWebInformationCollection
    {
        public WebInformationCollection(PnPContext context, IDataModelParent parent) : base(context, parent)
        {
            this.PnPContext = context;
            this.Parent = parent;
        }
    }
}