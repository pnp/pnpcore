using PnP.Core.QueryModel.Model;
using PnP.Core.Services;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Collection of Form Domain Model objects
    /// </summary>
    internal partial class FormCollection : QueryableDataModelCollection<IForm>, IFormCollection
    {
        public FormCollection(PnPContext context, IDataModelParent parent) : base(context, parent)
        {
            this.PnPContext = context;
            this.Parent = parent;
        }
    }
}