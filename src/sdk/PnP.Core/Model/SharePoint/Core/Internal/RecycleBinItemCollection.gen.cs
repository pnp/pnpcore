using PnP.Core.QueryModel;
using PnP.Core.Services;
using System;
using System.Threading.Tasks;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Collection of RecycleBinItem Domain Model objects
    /// </summary>
    internal partial class RecycleBinItemCollection : QueryableDataModelCollection<IRecycleBinItem>, IRecycleBinItemCollection
    {
        public RecycleBinItemCollection(PnPContext context, IDataModelParent parent, string memberName=null) : base(context, parent, memberName)
        {
            this.PnPContext = context;
            this.Parent = parent;
        }
    }
}