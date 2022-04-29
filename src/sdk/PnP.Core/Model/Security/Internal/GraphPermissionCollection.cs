using PnP.Core.QueryModel;
using PnP.Core.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace PnP.Core.Model.Security
{
    internal sealed class GraphPermissionCollection : QueryableDataModelCollection<IGraphPermission>, IGraphPermissionCollection
    {
        public GraphPermissionCollection(PnPContext context, IDataModelParent parent, string memberName = null) : base(context, parent, memberName)
        {
            PnPContext = context;
            Parent = parent;
        }
    }
}
