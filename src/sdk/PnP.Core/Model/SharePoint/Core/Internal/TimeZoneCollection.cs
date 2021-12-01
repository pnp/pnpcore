using PnP.Core.QueryModel;
using PnP.Core.Services;

namespace PnP.Core.Model.SharePoint
{
    internal sealed class TimeZoneCollection : QueryableDataModelCollection<ITimeZone>, ITimeZoneCollection
    {
        public TimeZoneCollection(PnPContext context, IDataModelParent parent, string memberName) : base(context, parent, memberName)
        {
            PnPContext = context;
            Parent = parent;
        }
    }
}
