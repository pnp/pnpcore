using PnP.Core.QueryModel.Model;
using PnP.Core.Services;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Collection of TimeZone Domain Model objects
    /// </summary>
    internal partial class TimeZoneCollection : QueryableDataModelCollection<ITimeZone>, ITimeZoneCollection
    {
        public TimeZoneCollection(PnPContext context, IDataModelParent parent) : base(context, parent)
        {
            this.PnPContext = context;
            this.Parent = parent;
        }
    }
}