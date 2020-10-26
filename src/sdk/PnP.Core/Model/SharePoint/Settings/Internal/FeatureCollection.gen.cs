using PnP.Core.QueryModel;
using PnP.Core.Services;

namespace PnP.Core.Model.SharePoint
{
    internal partial class FeatureCollection : QueryableDataModelCollection<IFeature>, IFeatureCollection
    {
        public FeatureCollection(PnPContext context, IDataModelParent parent, string memberName = null)
           : base(context, parent, memberName)
        {
            PnPContext = context;
            Parent = parent;
        }       
    }
}
