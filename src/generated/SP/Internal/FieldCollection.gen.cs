using PnP.Core.QueryModel.Model;
using PnP.Core.Services;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Collection of Field Domain Model objects
    /// </summary>
    internal partial class FieldCollection : QueryableDataModelCollection<IField>, IFieldCollection
    {
        public FieldCollection(PnPContext context, IDataModelParent parent) : base(context, parent)
        {
            this.PnPContext = context;
            this.Parent = parent;
        }
    }
}