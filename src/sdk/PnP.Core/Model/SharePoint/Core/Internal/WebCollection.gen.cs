using PnP.Core.QueryModel;
using PnP.Core.Services;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Collection of Web Domain Model objects
    /// </summary>
    internal partial class WebCollection : QueryableDataModelCollection<IWeb>, IWebCollection
    {
        public WebCollection(PnPContext context, IDataModelParent parent)
            : base(context, parent)
        {
            PnPContext = context;
            Parent = parent;
        }

    }
}
