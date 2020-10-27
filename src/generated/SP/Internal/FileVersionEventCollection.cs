using PnP.Core.QueryModel;
using PnP.Core.Services;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace PnP.Core.Model.SharePoint
{
    internal partial class FileVersionEventCollection : QueryableDataModelCollection<IFileVersionEvent>, IFileVersionEventCollection
    {
        public FileVersionEventCollection(PnPContext context, IDataModelParent parent, string memberName) : base(context, parent, memberName)
        {
            PnPContext = context;
            Parent = parent;
        }
    }
}
