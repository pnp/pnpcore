using PnP.Core.QueryModel.Model;
using PnP.Core.Services;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Collection of Feature Domain Model objects
    /// </summary>
    internal partial class FeatureCollection : QueryableDataModelCollection<IFeature>, IFeatureCollection
    {
        public FeatureCollection(PnPContext context, IDataModelParent parent) : base(context, parent)
        {
            this.PnPContext = context;
            this.Parent = parent;
        }
    }
}