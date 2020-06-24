using PnP.Core.QueryModel.Model;
using PnP.Core.Services;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Collection of TypeInformation Domain Model objects
    /// </summary>
    internal partial class TypeInformationCollection : QueryableDataModelCollection<ITypeInformation>, ITypeInformationCollection
    {
        public TypeInformationCollection(PnPContext context, IDataModelParent parent) : base(context, parent)
        {
            this.PnPContext = context;
            this.Parent = parent;
        }
    }
}